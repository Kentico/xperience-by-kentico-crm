namespace Kentico.Xperience.CRM.Common.Converters;

public interface ICRMTypeConverter<in TSource, in TDestination>
{
    Task Convert(TSource source, TDestination destination);
}