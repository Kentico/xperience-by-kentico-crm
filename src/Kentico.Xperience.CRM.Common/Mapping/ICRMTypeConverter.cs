namespace Kentico.Xperience.CRM.Common.Mapping;

public interface ICRMTypeConverter<in TSource, TDestination>
{
    Task<TDestination> Convert(TSource source, TDestination destination);
}