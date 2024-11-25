using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Product.Web.Models.Lookup
{
    public class ReferenceDataEdit
    {
        [Required]
        public int RefTypeId { get; set; }

        public dynamic RefValue { get; set; }
    }
}
