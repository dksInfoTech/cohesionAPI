using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Dal.Attributes;
public class DropDownAttribute : Attribute
{
    /// <summary>
    /// Lookup API name for the drop down list.
    /// </summary>
    public string LookupName { get; set; }
}

