
using Product.Dal.Entities;

namespace Product.Web.Models.Rule;
public class RuleExceptionResponse : RuleExceptionRequest
{
    public int Id { get; set; }
    public string ApproverName { get; set; }
    public string Requestor { get; set; }
    public Image? RequestorImage { get; set; }
    public Image? ApproverImage { get; set; }
}

