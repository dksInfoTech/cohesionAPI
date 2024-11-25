using System.Linq.Dynamic;
using Product.Web.Models.AGGrid;

namespace Product.Web.Extensions;

public static class AGGridQueryableExtensions
{
    /// <summary>
    /// Apply ag-grid paging to the IQueryable of T.
    /// </summary>
    /// <typeparam name="T">Data model.</typeparam>
    /// <param name="query">LINQ query.</param>
    /// <param name="agGridQuery">Grid query conditions defined by ag-grid.</param>
    /// <returns></returns>
    public static IQueryable<T> ApplyAGGridPaging<T>(this IQueryable<T> query, AGGridQuery agGridQuery) where T : class
    {
        if (agGridQuery != null && agGridQuery.EndRow > agGridQuery.StartRow)
        {
            query = query.Skip(agGridQuery.StartRow).Take(agGridQuery.EndRow - agGridQuery.StartRow);
        }

        return query;
    }

    /// <summary>
    /// Apply ag-grid sorting to the IQueryable of T. Note this implements System.Linq.Dynamic.Core which can result in runtime errors if property names do not match the model T.
    /// </summary>
    /// <typeparam name="T">Data model.</typeparam>
    /// <param name="query">LINQ query.</param>
    /// <param name="agGridQuery">Grid query conditions defined by ag-grid.</param>
    /// <returns></returns>
    public static IQueryable<T> ApplyAGGridSort<T>(this IQueryable<T> query, AGGridQuery agGridQuery) where T : class
    {
        if (agGridQuery?.SortModel != null && agGridQuery.SortModel.Any())
        {
            var sortString = string.Join(',', agGridQuery.SortModel.Select(x => x.ColId + " " + x.Sort));
            query = query.OrderBy(sortString);
        }

        return query;
    }

    /// <summary>
    /// Apply ag-grid filter query to the IQueryable of T. Note this implements System.Linq.Dynamic.Core which can result in runtime errors if property names do not match the model T.
    /// </summary>
    /// <typeparam name="T">Data model.</typeparam>
    /// <param name="query">LINQ query.</param>
    /// <param name="agGridQuery">Grid query conditions defined by ag-grid.</param>
    /// <returns></returns>
    public static IQueryable<T> ApplyAGGridFilter<T>(this IQueryable<T> query, AGGridQuery agGridQuery) where T : class
    {
        if (agGridQuery?.FilterModel != null && agGridQuery.FilterModel.Any())
        {
            foreach (var f in agGridQuery.FilterModel)
            {
                if (f.Value.FilterType == FilterTypeEnum.Set)
                {
                    if (f.Value != null && f.Value.Values.Any())
                    {
                        if (IsMultiValueField(typeof(T), f.Key))
                        {
                            // Special case for the Set filter when there are multiple values in a columns
                            // This requires multiple "contains" queries in SQL which is not very efficient but probably acceptable over the profile data volume

                            // Exclude null values first (AND condition)
                            query = query.Where($"{f.Key} != null");

                            // Filter on matches (one or more OR conditions)
                            string containsQuery = string.Join(" || ", f.Value.Values.Select((val, index) => $"{f.Key}.Contains(@{index})"));
                            query = query.Where(containsQuery, f.Value.Values.ToArray());
                        }
                        else if (IsBooleanField(typeof(T), f.Key))
                        {
                            // Boolean fields support "Y" and "N" values
                            var booleanValues = f.Value.Values
                                .Where(x => x.Equals("Y", StringComparison.OrdinalIgnoreCase) || x.Equals("N", StringComparison.OrdinalIgnoreCase))
                                .Select(x => x.ToUpper())
                                .Distinct();

                            if (booleanValues.Any())
                            {
                                // Query on: field == true || field == false
                                string booleanQuery = string.Join(" || ", booleanValues.Select(x => $"{f.Key} == {x.Equals("Y")}"));
                                query = query.Where(booleanQuery);
                            }
                        }
                        else
                        {
                            // Normal scenario for Set filter uses "in" SQL query
                            query = query.Where($"{f.Key} in @0", f.Value.Values);
                        }
                    }
                }
                else if (f.Value.FilterType == FilterTypeEnum.Date)
                {
                    // Date filter is configured to compare the date component and ignore the time component
                    // This is done by appending C# ".Date" for non-nullable data or ".Value.Date" for a nullable date 
                    string getDateExt = Nullable.GetUnderlyingType(typeof(T).GetProperty(f.Key).PropertyType) == null ? ".Date" : ".Value.Date";

                    switch (f.Value.Type)
                    {
                        case TypeEnum.Equals:
                            query = query.Where($"{f.Key}{getDateExt} == @0", f.Value.DateFrom);
                            break;
                        case TypeEnum.NotEqual:
                            query = query.Where($"{f.Key}{getDateExt} != @0", f.Value.DateFrom);
                            break;
                        case TypeEnum.LessThan:
                            query = query.Where($"{f.Key}{getDateExt} < @0", f.Value.DateFrom);
                            break;
                        case TypeEnum.LessThanOrEqual:
                            query = query.Where($"{f.Key}{getDateExt} <= @0", f.Value.DateFrom);
                            break;
                        case TypeEnum.GreaterThan:
                            query = query.Where($"{f.Key}{getDateExt} > @0", f.Value.DateFrom);
                            break;
                        case TypeEnum.GreaterThanOrEqual:
                            query = query.Where($"{f.Key}{getDateExt} >= @0", f.Value.DateFrom);
                            break;
                        case TypeEnum.InRange:
                            query = query.Where($"@0 <= {f.Key}{getDateExt} && {f.Key}{getDateExt} <= @1", f.Value.DateFrom, f.Value.DateTo);
                            break;
                        case TypeEnum.Empty:
                            query = query.Where($"{f.Key} == null");
                            break;
                        default:
                            break;
                    }
                }
                else if (f.Value.FilterType == FilterTypeEnum.Number)
                {
                    switch (f.Value.Type)
                    {
                        case TypeEnum.Equals:
                            query = query.Where($"{f.Key} == @0", f.Value.Filter);
                            break;
                        case TypeEnum.NotEqual:
                            query = query.Where($"{f.Key} != @0", f.Value.Filter);
                            break;
                        case TypeEnum.LessThan:
                            query = query.Where($"{f.Key} < @0", f.Value.Filter);
                            break;
                        case TypeEnum.LessThanOrEqual:
                            query = query.Where($"{f.Key} <= @0", f.Value.Filter);
                            break;
                        case TypeEnum.GreaterThan:
                            query = query.Where($"{f.Key} > @0", f.Value.Filter);
                            break;
                        case TypeEnum.GreaterThanOrEqual:
                            query = query.Where($"{f.Key} >= @0", f.Value.Filter);
                            break;
                        case TypeEnum.InRange:
                            query = query.Where($"@0 <= {f.Key} && {f.Key} <= @1", f.Value.Filter, f.Value.FilterTo);
                            break;
                        case TypeEnum.Empty:
                            query = query.Where($"{f.Key} == null");
                            break;
                        default:
                            break;
                    }
                }
                else // FilterTypeEnum.Text
                {
                    switch (f.Value.Type)
                    {
                        case TypeEnum.Equals:
                            query = query.Where($"{f.Key} == @0", f.Value.Filter);
                            break;
                        case TypeEnum.NotEqual:
                            query = query.Where($"{f.Key} != @0", f.Value.Filter);
                            break;
                        case TypeEnum.Contains:
                            query = query.Where($"{f.Key} != null && {f.Key}.Contains(@0)", f.Value.Filter);
                            break;
                        case TypeEnum.NotContains:
                            query = query.Where($"{f.Key} != null && !{f.Key}.Contains(@0)", f.Value.Filter);
                            break;
                        case TypeEnum.StartsWith:
                            query = query.Where($"{f.Key} != null && {f.Key}.StartsWith(@0)", f.Value.Filter);
                            break;
                        case TypeEnum.EndsWith:
                            query = query.Where($"{f.Key} != null && {f.Key}.EndsWith(@0)", f.Value.Filter);
                            break;
                        case TypeEnum.LessThan:
                            query = query.Where($"{f.Key} < @0", f.Value.Filter);
                            break;
                        case TypeEnum.LessThanOrEqual:
                            query = query.Where($"{f.Key} <= @0", f.Value.Filter);
                            break;
                        case TypeEnum.GreaterThan:
                            query = query.Where($"{f.Key} > @0", f.Value.Filter);
                            break;
                        case TypeEnum.GreaterThanOrEqual:
                            query = query.Where($"{f.Key} >= @0", f.Value.Filter);
                            break;
                        case TypeEnum.Empty:
                            query = query.Where($"{f.Key} == null");
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        return query;
    }

    /// <summary>
    /// Identify multi-value fields.
    /// </summary>
    /// <param name="t">Class type.</param>
    /// <param name="key">Property name.</param>
    /// <returns></returns>
    private static bool IsMultiValueField(Type t, string key)
    {
        return false;
    }

    /// <summary>
    /// Identify boolean fields.
    /// </summary>
    /// <param name="t">Class type.</param>
    /// <param name="key">Property name.</param>
    /// <returns></returns>
    private static bool IsBooleanField(Type t, string key)
    {
        return false;
    }
}
