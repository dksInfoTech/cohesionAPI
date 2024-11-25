namespace Product.Dal.Common.Models
{
    public class TemplateSection
    {
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string Tooltip { get; set; }

        public bool Collapsible { get; set; }

        /// <summary>
        /// Indicates that the section is hardcoded by system (true), or managed by the user (false)
        /// </summary>
        public bool SystemManaged { get; set; }

        public int Order { get; set; }

        public string Group { get; set; }

        public bool IsHidden { get; set; }

        public List<TemplateField> Fields { get; set; }

        public List<TemplateSection> Sections { get; set; }
    }
}
