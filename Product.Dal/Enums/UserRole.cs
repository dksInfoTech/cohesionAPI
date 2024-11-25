using System.ComponentModel;

namespace Product.Dal.Enums;

public enum UserRole
{
    [Description("R&A")]
    RnA,
    [Description("RM")]
    RM,
    [Description("Credit")]
    Credit,
    [Description("Support")]
    Support
}
