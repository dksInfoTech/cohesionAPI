using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;
using Product.Dal.Attributes;
using Product.Dal.Common.Models;

namespace Product.Dal.Common.Utils;

public class MetadataUtil
{
/// <summary>
        /// Get metadata using reflection (applies to the class properties with a DynmaicTemplate attribute).
        /// </summary>
        /// <param name="t">Type.</param>
        /// <param name="dynamicTemplateProperties">Only get metadata for dynamic template properties.</param>
        /// <returns></returns>
        public static List<PropertyMetadata> Get(Type t, bool dynamicTemplateProperties = false)
        {
            List<PropertyMetadata> metadata = new List<PropertyMetadata> { };

            foreach (var p in t.GetProperties())
            {
                // Ignore all properties except those with the DynmaicTemplate attribute
                if (dynamicTemplateProperties && !p.IsDefined(typeof(DynamicTemplateAttribute)))
                {
                    continue;
                }

                string type = "";
                var displayAttr = p.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute;
                var requiredAttr = p.GetCustomAttribute(typeof(RequiredAttribute)) as RequiredAttribute;
                var stringLengthAttr = p.GetCustomAttribute(typeof(StringLengthAttribute)) as StringLengthAttribute;
                var regexAttr = p.GetCustomAttribute(typeof(RegularExpressionAttribute)) as RegularExpressionAttribute;
                bool richText = p.IsDefined(typeof(RichTextAttribute));
                bool obsoleteAttr = p.IsDefined(typeof(ObsoleteAttribute));
                var layoutAttr = p.GetCustomAttribute(typeof(LayoutAttribute)) as LayoutAttribute;      // TODO: Can be removed now that it's managed in the TemplateField table
                bool jsonIgnoreAttr = p.IsDefined(typeof(JsonIgnoreAttribute));
                var dropDownAttr = p.GetCustomAttribute(typeof(DropDownAttribute)) as DropDownAttribute;

                // Exclude properties marked with JsonIgnore as these aren't included in the GET APIs anyway (SC-293)
                if (jsonIgnoreAttr)
                {
                    continue;
                }

                // Evaluate the "type" to show to the API consumer (a simplified version of .NET type names)
                if (richText)
                {
                    // Show "html" for a rich text editor field
                    type = "html";
                }
                else if (dropDownAttr != null)
                {
                    // Show "dropdown" for a Drop Down field
                    type = "dropdown";
                }
                else if (p.PropertyType.IsArray || (p.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(p.PropertyType)))
                {
                    // Show "array" for any type of collection (an array or a generic that is enumerable)
                    type = "array";
                }
                else
                {
                    // JLO 10/1/19: Get the underlying type if the property is nullable
                    Type underlyingType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

                    if (underlyingType == typeof(int))
                    {
                        // Show "int" instead of "Int32"
                        type = "int";
                    }
                    else if (underlyingType == typeof(long))
                    {
                        // Show "long" instead of "Int64"
                        type = "long";
                    }
                    else
                    {
                        // Show the type name in lowercase
                        type = underlyingType.Name.ToLower();
                    }
                }

                PropertyMetadata propertyMetdata = new PropertyMetadata
                {
                    Name = p.Name,
                    DisplayName = displayAttr != null ? displayAttr.Name : p.Name,
                    DisplayShortName = displayAttr != null ? displayAttr.ShortName : (displayAttr != null ? displayAttr.Name : p.Name),
                    Type = type,
                    Required = requiredAttr != null ? !requiredAttr.AllowEmptyStrings : false,
                    MinLength = stringLengthAttr != null ? stringLengthAttr.MinimumLength : (int?)null,
                    MaxLength = stringLengthAttr != null ? stringLengthAttr.MaximumLength : (int?)null,
                    Regex = regexAttr != null ? regexAttr.Pattern : null,
                    Obsolete = obsoleteAttr,
                    LayoutSection = layoutAttr != null ? layoutAttr.SectionName : null,
                    SectionShortName = layoutAttr?.SectionShortName != null ? layoutAttr.SectionShortName : (layoutAttr?.SectionName != null ? layoutAttr.SectionName : null),
                    LayoutOrder = layoutAttr != null ? layoutAttr.PropertyOrder : (int?)null,
                    SectionOrder = layoutAttr != null ? layoutAttr.SectionOrder : (int?)null
                };

                metadata.Add(propertyMetdata);
            }

            return metadata;
        }
}
