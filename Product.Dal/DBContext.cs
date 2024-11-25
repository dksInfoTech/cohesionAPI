using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Product.Dal.Entities;
using Product.Dal.Exceptions;

namespace Product.Dal;

public class DBContext : DbContext
{
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientDraft> ClientDrafts { get; set; }
    public DbSet<ClientTeamMember> ClientTeamMembers { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationReply> ConversationReplies { get; set; }
    public DbSet<Entity> Entities { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Proposal> Proposals { get; set; }
    public DbSet<ProposalEvent> ProposalEvents { get; set; }
    public DbSet<ProposalTeamMember> ProposalTeamMembers { get; set; }
    public DbSet<ReferenceData> ReferenceData { get; set; }
    public DbSet<ReferenceType> ReferenceType { get; set; }
    public DbSet<RoleCountry> RoleCountries { get; set; }
    public DbSet<RuleDefinition> Rules { get; set; }
    public DbSet<RuleOutcome> RuleOutcomes { get; set; }
    public DbSet<RuleQuery> RuleQueries { get; set; }
    public DbSet<RuleTrigger> RuleTriggers { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<ClientUserAccess> ClientUserAccessMapping { get; set; }
    public DbSet<ProposalHistory> ProposalHistory { get; set; }
    public DbSet<ClientHistory> ClientHistory { get; set; }
    public DbSet<Facility> Facilities { get; set; }
    public DbSet<FacilityDocument> FacilityDocuments { get; set; }
    public DbSet<InterchangableLimit> InterchangableLimits { get; set; }
    public DbSet<EventDashboard> EventDashboard { get; set; }
    public DbSet<PortfolioMonitor> PortfolioMonitors { get; set; }
    public DbSet<Configuration> Configurations { get; set; }

    public DbSet<EarlyAlert> EarlyAlerts { get; set; }
    public DbSet<EntityHierarchy> EntityHierarchy { get; set; }


    public DbSet<SourceEntity> SourceEntity { get; set; }
    public DbSet<SourceFinancialCode> SourceFinancialCode { get; set; }
    public DbSet<SourceFinancial> SourceFinancial { get; set; }
    public DbSet<SourceRating> SourceRating { get; set; }

    public DbSet<Entities.FinCodeGroup> FinCodeGroup { get; set; }
    public DbSet<Entities.FinCode> FinCode { get; set; }
    public DbSet<FinancialExtractJob> FinancialExtractJob { get; set; }
    public DbSet<FinancialExtractJobStatus> FinancialExtractJobStatus { get; set; }
    public DbSet<FinancialStatement> FinancialStatement { get; set; }
    public DbSet<Entities.Financial> Financial { get; set; }

    public DBContext(DbContextOptions<DBContext> options) : base(options)
    {
    }

    public DBContext() { }

    /// <summary>
    /// Set configuration options.
    /// </summary>
    /// <param name="dbContextOptionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder)
    {
        if (!dbContextOptionsBuilder.IsConfigured)
        {
            var connString = @"Host=localhost;Username=postgres;Password=password;Database=ProductDb";

            // Set lazy loading by default
            dbContextOptionsBuilder
                .AddInterceptors(new AuditSaveChangesInterceptor())
                .UseLazyLoadingProxies()
                .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning));

            dbContextOptionsBuilder.UseNpgsql(connString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        }
    }

    /// <summary>
    /// Overrides to apply for initial database creation.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call the same function on the base class to set the necessary properties on the base context
        base.OnModelCreating(modelBuilder);

        // Do not pluralise table names, set the names based on the class names
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.DisplayName());
        }

        // Turn off cascasding deletes globally (child entities must be deleted manually)
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,8)");
        }

        // Default dateTime type in Sql is datetime2, force it to datetime
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
        {
            property.SetColumnType("timestamp");
        }

        // Indexes on entities
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DBContext).Assembly);

        // modelBuilder.HasSequence<int>("FacilityIdSequence").IncrementsBy(1).StartsAt(1000).IsCyclic();

        // modelBuilder.HasSequence<int>("FacilityDocIdSequence").IncrementsBy(1).StartsAt(1000).IsCyclic();

        // modelBuilder.HasSequence<int>("ILIdSequence").IncrementsBy(1).StartsAt(1000).IsCyclic();

        // modelBuilder.Entity<Facility>()
        //     .Property(s => s.FacilityId)
        //     .HasDefaultValueSql("nextval('\"FacilityIdSequence\"')");

        // modelBuilder.Entity<FacilityDocument>()
        //     .Property(s => s.FacilityDocumentId)
        //     .HasDefaultValueSql("nextval('\"FacilityDocIdSequence\"')");

        // modelBuilder.Entity<InterchangableLimit>()
        //     .Property(s => s.InterchangableLimitId)
        //     .HasDefaultValueSql("nextval('\"ILIdSequence\"')");

        modelBuilder.Entity<SourceEntity>()
            .HasIndex(p => p.Ticker)
            .IsUnique();

        modelBuilder.Entity<SourceFinancial>()
            .HasKey(sf => new { sf.Ticker, sf.EqyFundYear, sf.FinCode });

        modelBuilder.Entity<SourceFinancial>()
            .HasOne(sf => sf.SourceEntity)
            .WithMany()
            .HasForeignKey(sf => sf.Ticker)
            .HasPrincipalKey(se => se.Ticker)
            .IsRequired();

        modelBuilder.Entity<SourceFinancial>()
            .HasOne(sf => sf.SourceFinancialCode)
            .WithMany()
            .HasForeignKey(sf => sf.FinCode)
            .HasPrincipalKey(se => se.FinCode)
            .IsRequired();

        modelBuilder.Entity<SourceRating>()
            .HasOne(sr => sr.SourceEntity)
            .WithMany()
            .HasForeignKey(sf => sf.Ticker)
            .HasPrincipalKey(se => se.Ticker)
            .IsRequired();

        modelBuilder.Entity<FinancialExtractJob>()
            .HasIndex(p => p.JobId)
            .IsUnique();

        modelBuilder.Entity<FinancialExtractJob>()
            .Property(p => p.Status)
            .HasConversion<string>();

        modelBuilder.Entity<FinancialExtractJob>()
            .Property(p => p.Stage)
            .HasConversion<string>();

        modelBuilder.Entity<FinancialExtractJob>()
            .HasOne(fej => fej.Entity)
            .WithMany()
            .HasForeignKey(fej => fej.EntityId)
            .HasPrincipalKey(e => e.Id)
            .IsRequired();

        modelBuilder.Entity<FinancialExtractJobStatus>()
            .HasOne(sr => sr.FinancialExtractJob)
            .WithMany()
            .HasForeignKey(fejs => fejs.ExtractJobId)
            .HasPrincipalKey(fej => fej.Id)
            .IsRequired();

        modelBuilder.Entity<FinancialExtractJobStatus>()
            .Property(p => p.Status)
            .HasConversion<string>();

        modelBuilder.Entity<FinancialExtractJobStatus>()
            .Property(p => p.Stage)
            .HasConversion<string>();

        modelBuilder.Entity<FinancialStatement>()
            .HasOne(fs => fs.Entity)
            .WithMany()
            .HasForeignKey(fs => fs.EntityId)
            .HasPrincipalKey(e => e.Id)
            .IsRequired();

        modelBuilder.Entity<FinancialStatement>()
            .HasOne(fs => fs.FinancialExtractJob)
            .WithMany()
            .HasForeignKey(fs => fs.ExtractJobId)
            .HasPrincipalKey(fej => fej.Id)
            .IsRequired();

        modelBuilder.Entity<FinancialStatement>()
            .Property(p => p.FinancialType)
            .HasConversion<string>();

        modelBuilder.Entity<Entities.Financial>()
            .HasOne(f => f.FinancialStatement)
            .WithMany()
            .HasForeignKey(f => f.FinancialStatementId)
            .HasPrincipalKey(fs => fs.Id)
            .IsRequired();

        modelBuilder.Entity<Entities.Financial>()
           .HasOne(f => f.Code)
           .WithMany()
           .HasForeignKey(f => f.FinCode)
           .HasPrincipalKey(fc => fc.Code)
           .IsRequired();

        modelBuilder.Entity<Entities.FinCode>()
           .HasOne(f => f.FinCodeGroup)
           .WithMany()
           .HasForeignKey(f => f.GroupCode)
           .HasPrincipalKey(fcg => fcg.GroupCode)
           .IsRequired();

        modelBuilder.Entity<FinCode>()
           .Property(p => p.FinStatementType)
           .HasConversion<string>();

        modelBuilder.Entity<FinCodeGroup>()
           .Property(p => p.FinStatementType)
           .HasConversion<string>();
    }
}
