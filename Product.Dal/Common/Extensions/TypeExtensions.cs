using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Product.Dal.Common.Extensions;
public static class TypeExtensions
{
    public static Type GetRealType<T>(this T source)
    {
        return typeof(T);
    }
}

public static class TypeUtil
{
    //a thread-safe way to hold default instances created at run-time
    private static ConcurrentDictionary<Type, object> typeDefaults =
       new ConcurrentDictionary<Type, object>();

    public static object GetDefaultValue(this Type type)
    {
        return type.IsValueType
           ? typeDefaults.GetOrAdd(type, Activator.CreateInstance)
           : type == typeof(string) ? string.Empty : null;
    }


    public static Type GetTemplateFieldType(string type)
    {
        switch (type)
        {
            case "number":
                return typeof(int);
            case "decimal":
                return typeof(double);
            case "dropdown":
            case "string":
            case "text":
            case "textarea":
            case "richtext":
                return typeof(string);
            case "datetime":
                return typeof(DateTime);
            default:
                return typeof(string);
        }
    }
}

