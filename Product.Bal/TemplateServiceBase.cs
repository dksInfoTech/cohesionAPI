using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Newtonsoft.Json;

using Newtonsoft.Json.Linq;
using Product.Bal.Interfaces;
using Product.Dal;
using Product.Dal.Attributes;
using Product.Dal.Common.Models;
using Product.Dal.Common.Utils;
using Product.Dal.Entities;
using Product.Dal.Interfaces;
using Product.Dal.Common.Extensions;

namespace Product.Bal;

/// <summary>
/// Optionally use this class as the base class of DbTemplateService in order to share common code.
/// </summary>
public abstract class TemplateServiceBase : ITemplateService
{
    private readonly Settings _settings;
    private readonly DBContext _db;

    public TemplateServiceBase(IOptions<Settings> settings, DBContext db)
    {
        _settings = settings.Value;
        _db = db;
    }

    public abstract ITemplateDefinition Get(string name, DateTime? asAtdate = null, bool getBaseMetadata = false);
    public abstract ITemplateDefinition Get(int? templateId, DateTime? asAtdate = null, bool getDefault = true, bool getBaseMetadata = false);
    public abstract ITemplateDefinition Get(Client client, int? templateId = null);

    /// <summary>
    /// Get the base model metadata to apply the type, min/max length, regex, and remove obsolete and hidden properties from the template.
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    protected void ApplyBaseMetadata(ITemplateDefinition template)
    {
        if (template?.TemplateData?.Sections == null)
        {
            return;
        }

        var templateData = template.TemplateData;
        var metadata = MetadataUtil.Get(typeof(Client));
        var obsoletePropertyNames = metadata.Where(x => x.Obsolete).Select(x => x.Name);
        IEnumerable<string> obsoleteSectionNames = new List<string> { };



        // Use recursion to navigate through all layers of "sections"
        ApplyBaseMetadataRecursion(templateData.Sections, metadata, obsoletePropertyNames, obsoleteSectionNames);

        // Set the template data
        template.TemplateData = templateData;
    }
    protected void ApplyBaseMetadata(ITemplateDefinition template, Client client)
    {
        if (template?.TemplateData?.Sections == null)
        {
            return;
        }

        var templateData = template.TemplateData;

        if (!string.IsNullOrWhiteSpace(client.BasicInformation))
        {
            var data = JObject.Parse(client.BasicInformation);
            var section = templateData.Sections.FirstOrDefault(s => s.Group == "Client");
            foreach (var k in data)
            {
                if (section.Fields.Any(f => f.Key == k.Key))
                {
                    object fieldValue = null;
                    switch (k.Value.Type)
                    {
                        case JTokenType.Integer:
                            fieldValue = Convert.ToInt32(k.Value.ToString());
                            break;
                        case JTokenType.Float:
                            fieldValue = Convert.ToDouble(k.Value.ToString());
                            break;
                        case JTokenType.String:
                            fieldValue = k.Value.ToString();
                            break;
                        case JTokenType.Boolean:
                            fieldValue = Convert.ToBoolean(k.Value.ToString());
                            break;
                        case JTokenType.Date:
                        case JTokenType.TimeSpan:
                            fieldValue = Convert.ToDateTime(k.Value.ToString());
                            break;
                    }
                    section.Fields.FirstOrDefault(f => f.Key == k.Key).Value = fieldValue;
                }
            }
        }

        // Set the template data
        template.TemplateData = templateData;
    }

    /// <summary>
    /// Recurisve function for ApplyBaseMetadata() above.
    /// </summary>
    /// <param name="sections"></param>
    /// <param name="metadata"></param>
    /// <param name="obsoletePropertyNames"></param>
    /// <param name="obsoleteSectionNames"></param>
    protected void ApplyBaseMetadataRecursion(List<TemplateSection> sections,
                                                IEnumerable<PropertyMetadata> metadata,
                                                IEnumerable<string> obsoletePropertyNames,
                                                IEnumerable<string> obsoleteSectionNames)
    {
        // Remove obsolete sections first
        sections.RemoveAll(x => obsoleteSectionNames.Contains(x.Name) || x.IsHidden);

        // Process each section
        foreach (var section in sections)
        {
            if (section.Fields != null)
            {
                // Remove hidden and obsolete fields
                section.Fields.RemoveAll(x => x.IsHidden || obsoletePropertyNames.Contains(x.Key));

                // Apply base metadata (type, min length, max length, regex)
                foreach (var field in section.Fields)
                {
                    var m = metadata.FirstOrDefault(x => x.Name == field.Key);

                    if (m != null)
                    {
                        field.Type = m.Type;
                        field.MinLength = m.MinLength;
                        field.MaxLength = m.MaxLength;
                        field.Regex = m.Regex;
                    }
                }
            }

            // Recurse through the next sections
            if (section.Sections != null)
            {
                ApplyBaseMetadataRecursion(section.Sections, metadata, obsoletePropertyNames, obsoleteSectionNames);
            }
        }
    }
}