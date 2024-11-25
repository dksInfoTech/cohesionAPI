using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Product.Dal.Exceptions;
using Product.Dal.Interfaces;

namespace Product.Dal;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var dbContext = eventData.Context;
        if (dbContext != null)
        {
            SaveAuditColumns(dbContext);
        }
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext != null)
        {
            SaveAuditColumns(dbContext);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SaveAuditColumns(DbContext dbContext)
    {
        var entities = from e in dbContext.ChangeTracker.Entries()
                       where e.State == EntityState.Added
                             || e.State == EntityState.Modified
                       select e.Entity;
        foreach (var entry in dbContext.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            if (entry.Entity is IAuditableEntity auditable)
            {               
                if (entry.State == EntityState.Added)
                {
                    auditable.Version++;
                    auditable.CreatedDate = DateTime.Now;
                    auditable.CreatedBy = "system";// logic to fetch current user;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.ModifiedDate = DateTime.Now;
                    auditable.ModifiedBy = "system";// logic to fetch current user;
                }
            }
            if (entry.Entity is IFacilityAuditableEntity auditableFacility)
            {               
                if (entry.State == EntityState.Added)
                {
                    auditableFacility.CreatedDate = DateTime.Now;
                    auditableFacility.CreatedBy = "system";// logic to fetch current user;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditableFacility.ModifiedDate = DateTime.Now;
                    auditableFacility.ModifiedBy = "system";// logic to fetch current user;
                }
            }
            if (entry.Entity is IAuditLog auditLog)
            {
                auditLog.Timestamp = DateTime.Now;
                auditLog.Username = "system";// logic to fetch current user;
                //auditLog.HostAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                //auditLog.HostName = _httpContextAccessor?.HttpContext?.Request?.Host.Host;
                // auditLog.Action = auditLog.Action.Truncate(500);
            }
        }
        ValidateEntities(dbContext);
    }

    /// <summary>
    /// Validate model entities before saving to the database. Throws EntityValidationException.
    /// </summary>
    private void ValidateEntities(DbContext dbContext)
    {
        var entities = from e in dbContext.ChangeTracker.Entries()
                       where e.State == EntityState.Added
                             || e.State == EntityState.Modified
                       select e.Entity;

        var entityValidationException = new EntityValidationException();
        var throwException = false;

        foreach (var entity in entities)
        {
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(entity);

            if (Validator.TryValidateObject(entity, validationContext, validationResults, true) == false)
            {
                throwException = true;
                entityValidationException.EntityValidationErrors.Add(new EntityValidationResult()
                {
                    ValidationErrors = validationResults.Select(x => new EntityValidationError
                    {
                        PropertyName = x.MemberNames.FirstOrDefault(),
                        ErrorMessage = x.ErrorMessage
                    }).ToList()
                });
            }
        }

        if (throwException)
        {
            throw entityValidationException;
        }
    }
}
