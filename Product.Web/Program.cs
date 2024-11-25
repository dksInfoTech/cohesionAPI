using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Product.Bal;
using Product.Dal;
using Product.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var startup = new Startup(builder.Environment);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    startup.Configure(app, builder.Environment, scope.ServiceProvider);
    var dbContext = scope.ServiceProvider.GetService<DBContext>();
    var userContextService = scope.ServiceProvider.GetService<UserContextService>();
    if (dbContext != null)
    {
        // Create the database and tables if it doesn't exist
        if (!dbContext.Database.GetService<IRelationalDatabaseCreator>().Exists())
        {
            dbContext.Database.Migrate();

            // Seed data
            SeedDataInitializer.Seed(dbContext, userContextService);
        }
        // if (!dbContext.Database.EnsureCreated())
        // {
        //     dbContext.Database.Migrate();
        // }
    }
}

app.UseCors();

app.UseHttpsRedirection();
app.MapControllers();
app.MapRazorPages();

app.Run();
