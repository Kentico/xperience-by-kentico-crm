using System.Globalization;

using Kentico.Xperience.CRM.Common.Metadata;

using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Kentico.Xperience.CRM.Dynamics.Metadata;

/// <inheritdoc />
internal class DynamicsAttributeValueConverter : IDynamicsAttributeValueConverter
{
    private readonly DynamicsEntityMetadataProvider metadataProvider;
    private readonly ILogger<DynamicsAttributeValueConverter> logger;

    public DynamicsAttributeValueConverter(DynamicsEntityMetadataProvider metadataProvider,
        ILogger<DynamicsAttributeValueConverter> logger)
    {
        this.metadataProvider = metadataProvider;
        this.logger = logger;
    }

    public async Task<object?> ConvertAsync(string entityLogicalName, string attributeName, object? value,
        CancellationToken cancellationToken = default)
    {
        var metadata = await metadataProvider.GetEntityMetadataAsync(entityLogicalName, cancellationToken);

        var field = metadata.Fields.Find(f =>
            string.Equals(f.Name, attributeName, StringComparison.OrdinalIgnoreCase));

        if (field is null)
        {
            // An attribute the metadata does not know about - most likely a custom attribute on an
            // environment whose schema could not be retrieved. Leave the value as it is.
            return value;
        }

        return Convert(field, value);
    }

    private object? Convert(CRMFieldMetadata field, object? value)
    {
        bool isText = string.Equals(field.NativeType, nameof(AttributeTypeCode.String),
                          StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(field.NativeType, nameof(AttributeTypeCode.Memo),
                          StringComparison.OrdinalIgnoreCase);

        // Text attributes keep the previous behavior, including empty strings, so that mappings defined in
        // code continue to behave exactly as before.
        if (isText)
        {
            return value is string || value is null ? value : ToInvariantString(value);
        }

        if (value is null || (value is string text && string.IsNullOrWhiteSpace(text)))
        {
            return null;
        }

        return field.NativeType switch
        {
            nameof(AttributeTypeCode.Integer) or nameof(AttributeTypeCode.BigInt) => ToInteger(value),
            nameof(AttributeTypeCode.Decimal) => ToDecimal(value),
            nameof(AttributeTypeCode.Double) => ToDouble(value),
            nameof(AttributeTypeCode.Money) => ToMoney(value),
            nameof(AttributeTypeCode.Boolean) => ToBoolean(value),
            nameof(AttributeTypeCode.DateTime) => ToDateTime(value),
            nameof(AttributeTypeCode.Uniqueidentifier) => ToGuid(value),
            nameof(AttributeTypeCode.Picklist) or nameof(AttributeTypeCode.State)
                or nameof(AttributeTypeCode.Status) => ToOptionSetValue(field, value),
            nameof(AttributeTypeCode.Lookup) or nameof(AttributeTypeCode.Customer)
                or nameof(AttributeTypeCode.Owner) => ToEntityReference(field, value),
            _ => value
        };
    }

    private object? ToInteger(object value)
    {
        if (value is int or long)
        {
            return value;
        }

        return int.TryParse(ToInvariantString(value), NumberStyles.Integer, CultureInfo.InvariantCulture,
            out int parsed)
            ? parsed
            : LogUnconvertible(value, nameof(AttributeTypeCode.Integer));
    }

    private object? ToDecimal(object value)
    {
        if (value is decimal)
        {
            return value;
        }

        return decimal.TryParse(ToInvariantString(value), NumberStyles.Number, CultureInfo.InvariantCulture,
            out decimal parsed)
            ? parsed
            : LogUnconvertible(value, nameof(AttributeTypeCode.Decimal));
    }

    private object? ToDouble(object value)
    {
        if (value is double)
        {
            return value;
        }

        return double.TryParse(ToInvariantString(value), NumberStyles.Float, CultureInfo.InvariantCulture,
            out double parsed)
            ? parsed
            : LogUnconvertible(value, nameof(AttributeTypeCode.Double));
    }

    private object? ToMoney(object value)
    {
        if (value is Money)
        {
            return value;
        }

        return decimal.TryParse(ToInvariantString(value), NumberStyles.Number, CultureInfo.InvariantCulture,
            out decimal amount)
            ? new Money(amount)
            : LogUnconvertible(value, nameof(AttributeTypeCode.Money));
    }

    private object? ToBoolean(object value)
    {
        if (value is bool)
        {
            return value;
        }

        string text = ToInvariantString(value) ?? string.Empty;

        return text.ToLowerInvariant() switch
        {
            "true" or "1" or "yes" or "y" => true,
            "false" or "0" or "no" or "n" => false,
            _ => LogUnconvertible(value, nameof(AttributeTypeCode.Boolean))
        };
    }

    private object? ToDateTime(object value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime;
        }

        return DateTime.TryParse(ToInvariantString(value), CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal, out var parsed)
            ? parsed
            : LogUnconvertible(value, nameof(AttributeTypeCode.DateTime));
    }

    private object? ToGuid(object value)
    {
        if (value is Guid)
        {
            return value;
        }

        return Guid.TryParse(ToInvariantString(value), out var parsed)
            ? parsed
            : LogUnconvertible(value, nameof(AttributeTypeCode.Uniqueidentifier));
    }

    /// <summary>
    /// Accepts either the numeric option value or the option label, because a marketer mapping a template
    /// or a constant onto an option set naturally writes the label they see in the CRM.
    /// </summary>
    private object? ToOptionSetValue(CRMFieldMetadata field, object value)
    {
        if (value is OptionSetValue)
        {
            return value;
        }

        string text = ToInvariantString(value) ?? string.Empty;

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int numeric))
        {
            return new OptionSetValue(numeric);
        }

        var option = field.Options.Find(o => string.Equals(o.Label, text, StringComparison.OrdinalIgnoreCase));

        if (option is not null &&
            int.TryParse(option.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int optionValue))
        {
            return new OptionSetValue(optionValue);
        }

        logger.LogWarning(
            "Value {Value} does not match any option of {Attribute}, the attribute is left unset",
            text, field.Name);

        return null;
    }

    /// <summary>
    /// Builds a reference from an identifier. Only possible when the attribute points at a single entity
    /// type, because otherwise the identifier alone does not say which record is meant.
    /// </summary>
    private object? ToEntityReference(CRMFieldMetadata field, object value)
    {
        if (value is EntityReference)
        {
            return value;
        }

        if (ToGuid(value) is not Guid id)
        {
            return null;
        }

        if (field.ReferenceTargets.Count != 1)
        {
            logger.LogWarning(
                "{Attribute} can reference {TargetCount} entity types, so the identifier {Id} cannot be resolved and the attribute is left unset",
                field.Name, field.ReferenceTargets.Count, id);

            return null;
        }

        return new EntityReference(field.ReferenceTargets[0], id);
    }

    private object? LogUnconvertible(object value, string attributeType)
    {
        logger.LogWarning("Value {Value} could not be converted to {AttributeType}, the attribute is left unset",
            value, attributeType);

        return null;
    }

    private static string? ToInvariantString(object? value) =>
        System.Convert.ToString(value, CultureInfo.InvariantCulture);
}