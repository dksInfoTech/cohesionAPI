using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Dal.Attributes;
    /// <summary>
    /// Defines the layout configuration of a property (section, field order etc).
    /// </summary>
    public class LayoutAttribute : Attribute
    {
        /// <summary>
        /// Section name for the property.
        /// </summary>
        public string SectionName { get; set; }

        /// <summary>
        /// Section short name for the property.
        /// </summary>
        public string SectionShortName { get; set; }

        /// <summary>
        /// Property order within the section.
        /// </summary>
        public int PropertyOrder { get; set; }

        /// <summary>
        /// Section order.
        /// </summary>
        public int SectionOrder { get; set; }
    }
