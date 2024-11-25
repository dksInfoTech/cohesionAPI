using Product.Dal.Common.Extensions;

namespace Product.Dal.Common.Data.Seed;

public class UserRoleSeedData
{
    public static List<Entities.UserRole> Get()
    {
        return new List<Entities.UserRole>
            {
                new Entities.UserRole
                {
                    Id = 1, Name = Enums.UserRole.RM.ToDescription(), Description = "Relationship Manager", CreatedBy = "system", ModifiedBy = "system", Version = 1
                },
                new Entities.UserRole
                {
                    Id = 2, Name = Enums.UserRole.RnA.ToDescription(), Description = "Research And Analysis",
                },
                new Entities.UserRole
                {
                    Id = 3, Name = Enums.UserRole.Credit.ToDescription(), Description = "Credit",
                },
                new Entities.UserRole
                {
                    Id = 4, Name = Enums.UserRole.Support.ToDescription(), Description = "Support",
                }
            };
    }
}
