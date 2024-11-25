using Product.Dal.Entities;

namespace Product.Dal.Common.Data.Seed;

public static class UserSeedData
{
    public static List<User> Get()
    {
        var supportRoleId = 4;
        var users = new List<User>
            {
                new User { Id = "system",Email="system@ja.com", FirstName = "System",LastName = "User", Password="1e2e9fc2002b002d75198b7503210c05a1baac4560916a3c6d93bcce3a50d7f00fd395bf1647b9abb8d1afcc9c76c289b0c9383ba386a956da4b38934417789e", Country = "Australia", Active = true, SystemAdmin = true, UserRoleId = supportRoleId},
                new User { Id = "john",Email="john@ja.com", FirstName = "John",LastName = "Argiro",  Password="1e2e9fc2002b002d75198b7503210c05a1baac4560916a3c6d93bcce3a50d7f00fd395bf1647b9abb8d1afcc9c76c289b0c9383ba386a956da4b38934417789e",Country = "Australia", Active = true, SystemAdmin = true, UserRoleId = 3 },
                new User { Id = "geoff",Email="geoff@ja.com", FirstName = "Geoffrey",LastName = "Bell",  Password="1e2e9fc2002b002d75198b7503210c05a1baac4560916a3c6d93bcce3a50d7f00fd395bf1647b9abb8d1afcc9c76c289b0c9383ba386a956da4b38934417789e",Country = "Australia", Active = true, SystemAdmin = true, UserRoleId = 2 },
                new User { Id = "karen",Email="karen@ja.com", FirstName = "Karen",LastName = "Mathers",  Password="1e2e9fc2002b002d75198b7503210c05a1baac4560916a3c6d93bcce3a50d7f00fd395bf1647b9abb8d1afcc9c76c289b0c9383ba386a956da4b38934417789e",Country = "Australia", Active = true, SystemAdmin = true, UserRoleId = 3 },
                new User { Id = "anthony",Email="anthony@ja.com", FirstName = "Anthony",LastName = "Bell", Password="1e2e9fc2002b002d75198b7503210c05a1baac4560916a3c6d93bcce3a50d7f00fd395bf1647b9abb8d1afcc9c76c289b0c9383ba386a956da4b38934417789e",Country = "Australia", Active = true, SystemAdmin = true, UserRoleId = 1 },
                new User { Id = "david",Email="david@ja.com", FirstName = "David",LastName = "Redhill", Password="1e2e9fc2002b002d75198b7503210c05a1baac4560916a3c6d93bcce3a50d7f00fd395bf1647b9abb8d1afcc9c76c289b0c9383ba386a956da4b38934417789e",Country = "Australia", Active = true, SystemAdmin = true, UserRoleId = 2 },
                };
        if (!users.Any(x => x.Id.Equals(Environment.UserName, StringComparison.OrdinalIgnoreCase)))
        {
            users.Add(new User { Id = Environment.UserName.ToLower(), Email = $"{Environment.UserName.ToLower()}@ja.com", FirstName = Environment.UserName.ToLower(), LastName = "test", Password = "1e2e9fc2002b002d75198b7503210c05a1baac4560916a3c6d93bcce3a50d7f00fd395bf1647b9abb8d1afcc9c76c289b0c9383ba386a956da4b38934417789e", Country = "Australia", Active = true, SystemAdmin = true, UserRoleId = supportRoleId });
        }
        return users;
    }
}
