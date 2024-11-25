namespace Product.Web.Models.Response;

public class ActionSaveResponse : ActionSaveResponse<object>
{

}

public class ActionSaveResponse<T> : ActionResponse where T : class
{
    public int ModelId { get; set; }

    public long ModelLongId { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string ModifiedDateString { get; set; }

    public T? RuleResponse { get; set; }
}
