namespace Kentico.Xperience.CRM.Common.Mapping;

/// <summary>
/// Kind of value expression used to produce the value written to a single CRM field.
/// </summary>
public enum ContactFieldValueKind
{
    /// <summary>
    /// Value is taken from a single contact field.
    /// </summary>
    SourceField = 0,

    /// <summary>
    /// Value is composed from literals and <c>{{ContactField}}</c> tokens.
    /// </summary>
    Template = 1,

    /// <summary>
    /// Value is a fixed literal.
    /// </summary>
    Constant = 2,

    /// <summary>
    /// Value is taken from the first contact field which is not empty.
    /// </summary>
    Coalesce = 3,

    /// <summary>
    /// Value is produced by a named <see cref="Resolvers.IContactSourceValueResolver"/>.
    /// </summary>
    Resolver = 4
}