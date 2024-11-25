using System.Reflection;
using AutoMapper;

namespace Product.Web.Extensions;

/// <summary>
/// AutoMapper mapping extensions.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Ignore non existing fields in AutoMapper maps.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
    {
        var flags = BindingFlags.Public | BindingFlags.Instance;
        var sourceType = typeof(TSource);
        var destinationProperties = typeof(TDestination).GetProperties(flags);

        foreach (var property in destinationProperties)
        {
            if (sourceType.GetProperty(property.Name, flags) == null)
            {
                expression.ForMember(property.Name, opt => opt.Ignore());
            }
        }

        return expression;
    }
}
