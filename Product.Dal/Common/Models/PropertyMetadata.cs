namespace Product.Dal.Common.Models;

public class PropertyMetadata
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string DisplayShortName { get; set; }

    public string Type { get; set; }

    public bool Required { get; set; }

    public int? MinLength { get; set; }

    public int? MaxLength { get; set; }

    public string Regex { get; set; }

    public bool Obsolete { get; set; }

    public string Tooltip { get; set; }

    public string LayoutSection { get; set; }

    public string SectionShortName { get; set; }

    public int? LayoutOrder { get; set; }

    public int? SectionOrder { get; set; }

    public string Placeholder { get; set; }
}
