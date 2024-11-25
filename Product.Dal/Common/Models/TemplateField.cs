using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Dal.Common.Models
{
    public class TemplateField
    {
        public bool IsRequiredInClient { get; set; }
        public bool? IsVisibleInDashboard { get; set; }        

        public bool? IsDisable { get; set; }

        public bool IsRequired { get; set; }

        public bool IsHidden { get; set; }

        public string Type { get; set; }

        public string Key { get; set; }

        public string LookupKey { get; set; }

        public string Name { get; set; }

        public bool ShowComments { get; set; }

        public string ShortName { get; set; }

        public bool Collapsible { get; set; }

        public int Order { get; set; }

        public string Tooltip { get; set; }

        public string DefaultValue { get; set; }

        public string Placeholder { get; set; }

        public int? MinLength { get; set; }

        public int? MaxLength { get; set; }

        public string Regex { get; set; }

        public bool SystemManaged { get; set; }

        public string Group { get; set; }

        public object Value { get; set; }

        public bool Multiple { get; set; }

        public string Externaldatamapping { get; set; }

        public string Externaldatasource { get; set; }

        public IDictionary<string, string> Options { get; set; }

        public IDictionary<string, string> Messages { get; set; }
    }
}
