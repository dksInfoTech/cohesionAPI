using System;
using System.Collections.Generic;
using System.Text;

namespace Product.Dal.Common.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Format date to string
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="format">Format string</param>
    /// <returns></returns>
    public static string ToFormatedString(this DateTime? dt, string format = null)
    {
        return string.Format(string.IsNullOrEmpty(format) ? "{0:dd/MM/yyyy}" : format, dt);
    }
}

