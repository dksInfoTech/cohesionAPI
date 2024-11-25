namespace Product.Web.Models.Response;

public class ActionResponse
{
    public ActionResponse()
    {
        Result = 0; // Success
    }

    public int Result { get; set; }

    public string Message { get; set; }
}

public class ActionResponse<T1, T2> where T1 : class
                                 where T2 : class
{
    public ActionResponse()
    {
        Result = 0; // Success
    }

    public int Result { get; set; }

    public string Message { get; set; }

    public T1 Data { get; set; }

    public T2 RuleResponse { get; set; }
}
