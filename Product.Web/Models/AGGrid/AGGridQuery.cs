using System;
using System.Collections.Generic;

namespace Product.Web.Models.AGGrid;

public class AGGridQuery
{
    public int StartRow { get; set; }

    public int EndRow { get; set; }

    public IEnumerable<SortModelSection> SortModel { get; set; }

    public Dictionary<string, FilterModelValue> FilterModel { get; set; }

    public class SortModelSection
    {
        public string ColId { get; set; }

        /// <summary>
        /// Values: "asc" or "desc".
        /// </summary>
        public string Sort { get; set; }
    }

    public class FilterModelValue
    {
        /// <summary>
        /// Values: "set" (dependent on Values), "text", "number", "date".
        /// </summary>
        public FilterTypeEnum FilterType { get; set; }

        /// <summary>
        /// Values for text  : "equals", "notEqual", "contains", "notContains", "startsWith", "endsWith", "empty".
        /// Values for number: "equals", "notEqual", "lessThan", "lessThanOrEqual", "greaterThan", "greaterThanOrEqual", "inRange", "empty".
        /// Values for date  : "equals", "notEqual", "lessThan", "greaterThan", "inRange", "empty".
        /// </summary>
        public TypeEnum Type { get; set; }

        /// <summary>
        /// Used by FilterType=="set".
        /// </summary>
        public IEnumerable<string> Values { get; set; }

        /// <summary>
        /// Used by FilterType=="text" and FilterType=="number".
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Used by FilterType=="number" when Type="inRange".
        /// </summary>
        public string FilterTo { get; set; }

        /// <summary>
        /// Used by FilterType=="date".
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Used by FilterType=="date" when Type="inRange".
        /// </summary>
        public DateTime? DateTo { get; set; }
    }
}
