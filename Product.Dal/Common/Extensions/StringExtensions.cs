using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Product.Dal.Common.Extensions;
public static class StringExtensions
{
    /// <summary>
    /// Remove the prefix "domain" from a Windows domain account.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string LanId(this string s)
    {
        if (!string.IsNullOrEmpty(s) && s.Contains('\\'))
        {
            return s.Split('\\')[1];
        }
        else
        {
            return s;
        }
    }

    /// <summary>
    /// Remove the prefix "domain" from a Windows domain account and add a suffix to create an email address.
    /// For accounts on the globaltest domain also remove any "admin" suffix so that emails will work from the standard global account.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string ToEmailAddress(this string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }

        s = s.ToLower();

        // Remove the domain prefix
        s = Regex.Replace(s, @"^.*\\", "");

        return s + "@product.com";
    }

    /// <summary>
    /// Convert from user@domain.com to domain\user format.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string ToDomainUser(this string s)
    {
        if (!string.IsNullOrEmpty(s))
        {
            // Convert from "user@domain.com" to "domain\user" format
            var temp = s.ToLower().Split('@');

            if (temp.Length == 2)
            {
                s = temp[1].Split('.')[0] + "\\" + temp[0];
            }
        }

        return s;
    }

    /// <summary>
    /// Truncate to a maximum length (without going out of bounds).
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    /// <summary>
    /// Remove HTML markup to obtain only the text.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string HtmlToText(this string value)
    {
        return Regex.Replace(value, @"<[^>]*>", " ");
    }

    /// <summary>
    /// Word count.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int WordCount(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }
        else
        {
            return Regex.Matches(value, @"[\S]+").Count;
        }
    }

    /// <summary>
    /// Word count based on HTML input string.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int HtmlWordCount(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }
        else
        {
            return Regex.Matches(value.HtmlToText(), @"[\S]+").Count;
        }
    }

    public static bool IsValidJson(this string s)
    {
        try
        {
            JToken.Parse(s);
            return true;
        }
        catch (JsonReaderException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
