using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Dal.Attributes;
public class StringRangeAttribute : ValidationAttribute
{
    /// <summary>
    /// String values that pass validation.
    /// </summary>
    public string[] AllowedValues { get; set; }

    /// <summary>
    /// Validation allows null or empty string (default true).
    /// </summary>
    public bool AllowNull { get; set; } = true;

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (AllowNull && string.IsNullOrEmpty(value?.ToString()))
        {
            return ValidationResult.Success;
        }

        if (AllowedValues?.Contains(value?.ToString()) == true)
        {
            return ValidationResult.Success;
        }

        var msg = "is invalid, allowed values are: " + string.Join(", ", AllowedValues ?? new string[] { });

        return new ValidationResult(msg, new List<string> { validationContext.MemberName });
    }
}
