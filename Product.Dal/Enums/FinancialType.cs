using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Dal.Enums
{
    public enum FinancialType
    {
        [Description("Annual")]
        Annual,
        [Description("Quarterly")]
        Quarterly,
        [Description("SemiAnnual")]
        SemiAnnual,
        [Description("YTD")]
        YTD
    }
}
