using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Product.Bal;
using Product.Bal.Interfaces;
using Product.Dal;
using Product.Dal.Entities;
using Product.Dapper.Lib;
using Product.Integration;
using Product.Integration.Interfaces;
using Product.Integration.Models;
using Product.Web.AutoMapper.Config;
using Product.Web.Config;
using Product.Web.Models.Configuration;
using System.Security.Policy;


namespace Product.Web;

public class Startup
{
    public IConfigurationRoot _configuration { get; }
    public Startup(IWebHostEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();
    }
    public void ConfigureServices(IServiceCollection services)
    {   
        // Load config from appsettings to class
        services.Configure<IntegrationConfig>(_configuration.GetSection("IntegrationConfig"));
        services.AddSingleton<IntegrationConfig>(provider => provider.GetRequiredService<IOptions<IntegrationConfig>>().Value);

        services.Configure<FdeConfig>(_configuration.GetSection("FdeConfig"));

        services.AddControllers();
        //services.AddLogging();
        services.AddEndpointsApiExplorer();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product API", Version = "v1" });
            var assembly = typeof(Startup).Assembly;
            var basePath = Path.GetDirectoryName(assembly.Location);
            var fileName = assembly.GetName().Name + ".xml";
            c.IncludeXmlComments(Path.Combine(basePath ?? "", fileName));
        });

        LoggingConfig.ConfigureLogging(services, _configuration);

        // Add configuration
        services.AddSingleton<IConfiguration>(_configuration);
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddRazorPages();
        services.AddHttpContextAccessor();

        // Add validation logic
        // services.AddFluentValidationAutoValidation();
        // services.AddTransient<IValidator<BlogModel>, BlogModelValidator>();

        services.AddOptions();

        // Settings
        services.Configure<Settings>(_configuration.Bind);

        // Configure CORS

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });

        // Get the connection string from configuration
        var dbConnectionString = _configuration.GetValue<string>("ConnectionStrings:dbConnection");

        // Register the DbContext with proper configurations
        services.AddDbContext<DBContext>(opts => opts
            .UseLazyLoadingProxies() // Enable lazy loading proxies
            .AddInterceptors(new AuditSaveChangesInterceptor())
            .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning)) // Configure warnings
            .UseNpgsql(dbConnectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)), // Use Npgsql with the connection string
            contextLifetime: ServiceLifetime.Transient); // Use Scoped lifetime (default)

        // Register IDbConnectionFactory and IDapperDbContext for Dapper usage
        services.AddTransient<IDbConnectionFactory>(s => new DbConnectionFactory(dbConnectionString));
        services.AddScoped<IDapperDbContext, DapperDbContext>();

        // Add AutoMapper service
        services.AddAutoMapper(typeof(AutoMapperConfig));
        
        services.AddSingleton<UserContextService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProposalService, ProposalService>();
        services.AddScoped<IClientUserAccessMappingService, ClientUserAccessMappingService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IRuleSetService, RuleSetService>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<IFinancialService, FinancialService>();

        // Register HttpClient (HttpClient is shared by DI across multiple services)
        services.AddHttpClient("DefaultClient").ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
        });

        // Register bloomberg auth service as a singleton
        //services.AddSingleton<IIntegrationAuthService, BloombergAuthService>();
        //services.AddSingleton<IIntegrationService, IntegrationService>();
        //services.AddSingleton<BloombergDataService>(sp =>
        //{
        //    return new BloombergDataService(sp.GetRequiredService<IIntegrationService>(), sp.GetRequiredService<IntegrationConfig>(), dbConnectionString);
        //});
        //services.AddSingleton<BloombergDataService, BloombergDataService>();

        //services.AddSingleton<ZeroMqPubService>();
        //services.AddHostedService<ZeroMqSubService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ZeroMqPubService>>();
        ZeroMqPubService.Initialize(logger);
    }
}
