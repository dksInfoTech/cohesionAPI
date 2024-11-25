using Product.Dal;
using Product.Dal.Common.Data.Seed;
using Product.Dal.Entities;

namespace Product.Bal;

public class SeedDataInitializer
{
    public static void Seed(DBContext context, UserContextService? userContextService)
    {
        // User Classification Business Roles data
        context.UserRoles.AddRange(UserRoleSeedData.Get());
        context.SaveChanges();

        // User Country White list data
        context.RoleCountries.AddRange(RoleCountrySeedData.Get());
        context.SaveChanges();


        // Add user permissions data
        context.Permissions.AddRange(PermissionSeedData.Get());
        context.SaveChanges();

        // Add initial list of users
        context.Users.AddRange(UserSeedData.Get());
        context.SaveChanges();

        // Templates
        context.Templates.AddRange(TemplateSeedData.Get());
        context.SaveChanges();

        // Reference Type
        context.ReferenceType.AddRange(ReferenceSeedData.GetReferenceTypes());
        context.SaveChanges();

        // Reference Data
        context.ReferenceData.AddRange(ReferenceSeedData.GetReferenceData(context));
        context.SaveChanges();

        context.Images.AddRange(ClientImageSeedData.Get());
        context.SaveChanges();

        // Clients
        context.Clients.AddRange(ClientSeedData.Get());
        context.SaveChanges();

        var clientLoopkup = context.Clients.ToDictionary(x => x.Name, x => x.Id);

        // Proposals
        context.Proposals.AddRange(ProposalSeedData.Get(clientLoopkup));
        context.SaveChanges();

        var proposalLoopkup = context.Proposals.ToDictionary(x => x.ClientId, x => x.Id);

        // ClientDrafts
        context.ClientDrafts.AddRange(ClientDraftSeedData.Get(clientLoopkup, proposalLoopkup));
        context.SaveChanges();
        
        // RefreshAllPermissions(context);
    }

    private static void RefreshAllPermissions(DBContext context)
    {
        foreach (var user in context.Users.Select(x => x.Id).ToList())
        {
            foreach (var clientId in context.Clients.Select(client => client.Id).ToList())
            {
                context.ClientUserAccessMapping.Add(

                        new ClientUserAccess
                        {
                            ClientId = clientId,
                            UserId = user,
                            Admin = true,
                        });
            }
        }
        context.SaveChanges();
    }
}
