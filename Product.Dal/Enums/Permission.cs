using System.ComponentModel;

namespace Product.Dal.Enums;

public enum Permission
{
    [Description("Default")]
    Default,
    [Description("SystemAdmin")]
    SystemAdmin,
    [Description("ClientCreator")]
    ClientCreator,
    [Description("ClientRead")]
    ClientRead,
    [Description("ClientWrite")]
    ClientWrite,
    [Description("ClientApproval")]
    ClientApproval,
    [Description("ClientAdmin")]
    ClientAdmin,
    [Description("CreateEditUsers")]
    CreateEditUsers
}
