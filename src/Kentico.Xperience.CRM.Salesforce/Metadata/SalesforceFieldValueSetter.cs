using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;

namespace Kentico.Xperience.CRM.Salesforce.Metadata;

/// <inheritdoc />
internal class SalesforceFieldValueSetter : ISalesforceFieldValueSetter
{
    /// <summary>
    /// Salesforce field name to generated property, per sObject type. Reflection over an sObject with
    /// dozens of properties runs for every synchronized contact, so the map is built once.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> PropertyMaps =
        new();

    private readonly SalesforceEntityMetadataProvider metadataProvider;
    private readonly ILogger<SalesforceFieldValueSetter> logger;

    public SalesforceFieldValueSetter(SalesforceEntityMetadataProvider metadataProvider,
        ILogger<SalesforceFieldValueSetter> logger)
    {
        this.metadataProvider = metadataProvider;
        this.logger = logger;
    }

    public async Task SetFieldAsync(object sObject, IDictionary<string, object> additionalProperties,
        string entityType, string fieldName, object? value, CancellationToken cancellationToken = default)
    {
        var property = GetPropertyMap(sObject.GetType()).GetValueOrDefault(fieldName);

        if (property is not null)
        {
            // A field the generated class declares is set through the property, so it is serialized with
            // the right JSON type and cannot collide with the extension data.
            if (TryConvert(value, property.PropertyType, out object? converted))
            {
                property.SetValue(sObject, converted);
            }
            else
            {
                logger.LogWarning("Value {Value} could not be converted to {Type} for field {Field}",
                    value, property.PropertyType.Name, fieldName);
            }

            return;
        }

        // Otherwise the field is not part of the generated class - a custom field, most likely. The
        // describe metadata says which JSON type Salesforce expects.
        additionalProperties[fieldName] =
            await ConvertCustomFieldAsync(entityType, fieldName, value, cancellationToken) ?? string.Empty;
    }

    private async Task<object?> ConvertCustomFieldAsync(string entityType, string fieldName, object? value,
        CancellationToken cancellationToken)
    {
        var metadata = await metadataProvider.GetEntityMetadataAsync(entityType, cancellationToken);

        var field = metadata.Fields.Find(f =>
            string.Equals(f.Name, fieldName, StringComparison.OrdinalIgnoreCase));

        var targetType = field?.NativeType.ToLowerInvariant() switch
        {
            "boolean" => typeof(bool?),
            "int" or "long" => typeof(int?),
            "double" or "currency" or "percent" => typeof(double?),
            "date" or "datetime" => typeof(DateTimeOffset?),
            _ => typeof(string)
        };

        if (TryConvert(value, targetType, out object? converted))
        {
            return converted;
        }

        logger.LogWarning("Value {Value} could not be converted for field {Field}, sending it as text",
            value, fieldName);

        return ToInvariantString(value);
    }

    private static IReadOnlyDictionary<string, PropertyInfo> GetPropertyMap(Type sObjectType) =>
        PropertyMaps.GetOrAdd(sObjectType, type => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .GroupBy(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Converts a mapped value to the target type. Values which already have the target type pass through,
    /// which keeps mappings defined in code behaving exactly as before.
    /// </summary>
    private static bool TryConvert(object? value, Type targetType, out object? converted)
    {
        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value is null)
        {
            converted = null;
            return true;
        }

        if (type.IsInstanceOfType(value))
        {
            converted = value;
            return true;
        }

        string text = ToInvariantString(value) ?? string.Empty;

        if (type == typeof(string))
        {
            converted = text;
            return true;
        }

        // An empty value cannot be represented by any of the value types below, and Salesforce rejects an
        // empty string for them, so the field is left unset.
        if (string.IsNullOrWhiteSpace(text))
        {
            converted = null;
            return true;
        }

        if (type == typeof(bool))
        {
            return TryConvertBoolean(text, out converted);
        }

        if (type == typeof(int))
        {
            bool parsed = int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out int intValue);
            converted = parsed ? intValue : null;
            return parsed;
        }

        if (type == typeof(long))
        {
            bool parsed = long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out long longValue);
            converted = parsed ? longValue : null;
            return parsed;
        }

        if (type == typeof(double))
        {
            bool parsed = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture,
                out double doubleValue);
            converted = parsed ? doubleValue : null;
            return parsed;
        }

        if (type == typeof(decimal))
        {
            bool parsed = decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture,
                out decimal decimalValue);
            converted = parsed ? decimalValue : null;
            return parsed;
        }

        if (type == typeof(DateTimeOffset))
        {
            bool parsed = DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal, out var dateTimeOffsetValue);
            converted = parsed ? dateTimeOffsetValue : null;
            return parsed;
        }

        if (type == typeof(DateTime))
        {
            bool parsed = DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal,
                out var dateTimeValue);
            converted = parsed ? dateTimeValue : null;
            return parsed;
        }

        if (type == typeof(object))
        {
            converted = value;
            return true;
        }

        converted = null;
        return false;
    }

    private static bool TryConvertBoolean(string text, out object? converted)
    {
        switch (text.ToLowerInvariant())
        {
            case "true":
            case "1":
            case "yes":
            case "y":
                converted = true;
                return true;

            case "false":
            case "0":
            case "no":
            case "n":
                converted = false;
                return true;

            default:
                converted = null;
                return false;
        }
    }

    private static string? ToInvariantString(object? value) =>
        Convert.ToString(value, CultureInfo.InvariantCulture);
}