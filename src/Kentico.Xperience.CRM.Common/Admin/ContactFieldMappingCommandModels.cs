namespace Kentico.Xperience.CRM.Common.Admin;

/// <summary>
/// Arguments of the commands which act on a single CRM entity type.
/// </summary>
public class ChangeEntityTypeArguments
{
    /// <summary>
    /// Entity type to load. See <see cref="Constants.EntityType"/>.
    /// </summary>
    public string? EntityType { get; set; }
}

/// <summary>
/// Arguments of the save command.
/// </summary>
public class SaveMappingsArguments
{
    /// <summary>
    /// Entity type the mappings belong to. See <see cref="Constants.EntityType"/>.
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Complete set of mappings for the entity type. An empty set removes the configuration.
    /// </summary>
    public List<ContactFieldMappingRowModel> Mappings { get; set; } = new();
}

/// <summary>
/// Arguments of the preview command.
/// </summary>
public class PreviewArguments
{
    /// <summary>
    /// Row to evaluate. Does not have to be saved.
    /// </summary>
    public ContactFieldMappingRowModel? Mapping { get; set; }
}

/// <summary>
/// Everything that changes when the marketer switches entity type or saves.
/// </summary>
public class ContactFieldMappingDataModel
{
    public string EntityType { get; set; } = string.Empty;

    public List<CRMTargetFieldModel> CrmFields { get; set; } = new();

    public List<ContactFieldMappingRowModel> Mappings { get; set; } = new();

    public bool MetadataIsLive { get; set; }

    public string? MetadataWarning { get; set; }

    public bool HasConfiguration { get; set; }
}

/// <summary>
/// Result of the load defaults command.
/// </summary>
public class LoadDefaultsResultModel
{
    public List<ContactFieldMappingRowModel> Mappings { get; set; } = new();
}

/// <summary>
/// Result of the preview command.
/// </summary>
public class PreviewResultModel
{
    /// <summary>
    /// Value the expression produced.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Contact the value was produced from, so the marketer knows what they are looking at.
    /// </summary>
    public string? SampleContact { get; set; }

    /// <summary>
    /// Reason the preview could not be produced.
    /// </summary>
    public string? Error { get; set; }
}