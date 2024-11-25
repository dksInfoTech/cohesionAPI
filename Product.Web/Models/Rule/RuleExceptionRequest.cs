namespace Product.Web.Models.Rule;
public class RuleExceptionRequest
{
    public string Status { get; set; }

    public string Reason { get; set; }

    public int SourceId { get; set; }

    public DateTime DueDate { get; set; }

    public string Approver { get; set; }
}
