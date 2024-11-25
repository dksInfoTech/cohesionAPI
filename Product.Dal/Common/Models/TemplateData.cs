using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Dal.Common.Models
{
    public class TemplateData
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<TemplateSection> Sections { get; set; }

        /// <summary>
        /// Find a template section (null if not exists).
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public TemplateSection FindSection(string sectionName)
        {
            return Sections?.Where(x => x.Sections != null).SelectMany(x => x.Sections).Where(x => x?.Name != null && x.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// Find a template field (null if not exists).
        /// </summary>
        /// <param name="key">Field key</param>
        /// <returns></returns>
        public TemplateField FindField(string key)
        {
            return GetField(Sections, key);
        }

        /// <summary>
        /// Find a template field (null if not exists).
        /// </summary>
        /// <param name="sections">List of section</param>
        /// <param name="key">Field key</param>
        /// <returns></returns>
        private TemplateField GetField(List<TemplateSection> sections, string key)
        {
            if (sections == null)
            {
                return null;
            }

            foreach (var section in sections)
            {
                if (section.Fields != null && section.Fields.Any())
                {
                    foreach (var field in section.Fields)
                    {
                        if (field.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            return field;
                        }
                    }
                }

                if (section.Sections != null && section.Sections.Any())
                {
                    var field = GetField(section.Sections, key);

                    if (field != null)
                    {
                        return field;
                    }
                }
            }

            return null;
        }
    }

}
