using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Product.Dal.Common.Extensions;
public static class JsonExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static T JsonToObject<T>(this string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return default(T);
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(s);
        }
        catch (Exception)
        {
            return default(T);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static List<KeyValuePair<string, string>> JsonToKeyValuePair(this string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(s);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string JsonToUserList(this string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return null;
        }

        var list = s.JsonToKeyValuePair();

        if (list == null || !list.Any())
        {
            return null;
        }

        return string.Join("; ", list.Select(x => $"{x.Value} ({x.Key})"));
    }
}
