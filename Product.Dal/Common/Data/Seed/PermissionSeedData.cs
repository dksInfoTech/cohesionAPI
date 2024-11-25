using Product.Dal.Common.Extensions;

namespace Product.Dal.Common.Data.Seed;

public static class PermissionSeedData
    {
        public static List<Entities.Permission> Get()
        {
            return new List<Entities.Permission>
            {
                new Entities.Permission { UserRoleId = 1, UserPermission = Enums.Permission.CreateEditUsers.ToDescription() },
                new Entities.Permission { UserRoleId = 1, UserPermission = Enums.Permission.ClientRead.ToDescription() },
                new Entities.Permission { UserRoleId = 1, UserPermission = Enums.Permission.ClientWrite.ToDescription() },
                new Entities.Permission { UserRoleId = 1, UserPermission = Enums.Permission.ClientApproval.ToDescription() },

                new Entities.Permission { UserRoleId = 2, UserPermission = Enums.Permission.CreateEditUsers.ToDescription() },
                new Entities.Permission { UserRoleId = 2, UserPermission = Enums.Permission.ClientRead.ToDescription() },
                new Entities.Permission { UserRoleId = 2, UserPermission = Enums.Permission.ClientWrite.ToDescription() },
                new Entities.Permission { UserRoleId = 2, UserPermission = Enums.Permission.ClientApproval.ToDescription() },

                new Entities.Permission { UserRoleId = 3, UserPermission = Enums.Permission.CreateEditUsers.ToDescription() },
                new Entities.Permission { UserRoleId = 3, UserPermission = Enums.Permission.ClientRead.ToDescription() },
                new Entities.Permission { UserRoleId = 3, UserPermission = Enums.Permission.ClientWrite.ToDescription() },
                new Entities.Permission { UserRoleId = 3, UserPermission = Enums.Permission.ClientApproval.ToDescription() },

                new Entities.Permission { UserRoleId = 4, UserPermission = Enums.Permission.CreateEditUsers.ToDescription() },
                new Entities.Permission { UserRoleId = 4, UserPermission = Enums.Permission.ClientRead.ToDescription() },
                new Entities.Permission { UserRoleId = 4, UserPermission = Enums.Permission.ClientWrite.ToDescription() },
                new Entities.Permission { UserRoleId = 4, UserPermission = Enums.Permission.ClientApproval.ToDescription() },
                
            };
        }
    }
