using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Product.Web.Models.Lookup
{
    public class ReferenceDataViewModel
    {
        public int RefDataId { get; set; }
        public int RefTypeId { get; set; }
        public string RefKey { get; set; }
        public bool IsFilteringAllowed { get; set; }       
        public dynamic RefValue { get; set; }
    }
}
