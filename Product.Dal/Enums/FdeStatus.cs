using System.ComponentModel;

namespace Product.Dal.Enums
{
    public enum FdeStatus
    {
        [Description("Success")]
        Success,
        [Description("Error")]
        Error,
        [Description("InProgress")]
        InProgress
    }
}
