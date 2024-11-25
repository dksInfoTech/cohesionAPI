using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Product.Bal.Interfaces;
using Product.Bal.Models;
using Product.Dal;
using Product.Dal.Entities;
using Product.Dal.Enums;
using System.Xml;

namespace Product.Bal
{
    public class FinancialService : IFinancialService
    {
        private readonly DBContext _db;
        private readonly ILogger<FinancialService> _logger;

        public FinancialService(DBContext db, ILogger<FinancialService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> UpdateFinancialExtractJobStatus(FdeStatusUpdate response)
        {
            var entityId = int.Parse(response.EntityId);

            using var transaction = await _db.Database.BeginTransactionAsync();  // Begin a transaction

            try
            {
                var job = _db.FinancialExtractJob.FirstOrDefault(x => x.EntityId == entityId && x.JobId == response.JobId);
                if (job == null)
                {
                    return false;
                }

                var fdeStage = (FdeStage)Enum.Parse(typeof(FdeStage), response.Stage);
                var fdeStatus = (FdeStatus)Enum.Parse(typeof(FdeStatus), response.Status);

                job.Stage = fdeStage;

                if (fdeStage == FdeStage.OutputBs || fdeStatus == FdeStatus.Error)
                {
                    job.IsCompleted = true;
                    job.Status = fdeStatus;
                    job.FinishedAt = DateTime.UtcNow.ToLocalTime();

                    if (fdeStatus == FdeStatus.Success)
                    {
                        job.Stage = FdeStage.Completed;
                    }
                }

                var jobStatus = new FinancialExtractJobStatus
                {
                    ExtractJobId = job.Id,
                    Stage = fdeStage,
                    Status = fdeStatus,
                    UpdatedAt = DateTime.UtcNow.ToLocalTime(),
                    Details = response.message
                };

                _db.FinancialExtractJob.Update(job);
                _db.FinancialExtractJobStatus.Add(jobStatus);
                await _db.SaveChangesAsync();

                // Prepare FinancialStatement and Financial entries
                var financialStatements = new List<FinancialStatement>();

                if (job.Stage == FdeStage.Completed && response.Content?.Count > 0)
                {
                    foreach (var statement in response.Content)
                    {
                        if (statement.Content != null)
                        {
                            if (!statement.FinYear.ToLower().Contains("parent"))
                            {
                                var finYear = int.Parse(statement.FinYear.Split('_').Last());

                                var finStatement = new FinancialStatement
                                {
                                    EntityId = entityId,
                                    ExtractJobId = job.Id,
                                    FinancialType = FinancialType.Annual,
                                    FinancialYear = finYear
                                };

                                //_db.FinancialStatement.Add(finStatement);
                                var statementEntity = await UpsertFinancialStatement(finStatement);
                                financialStatements.Add(statementEntity);
                            }
                        }
                    }
                    await _db.SaveChangesAsync();

                    foreach (var finStatement in financialStatements)
                    {
                        var statement = response.Content.Where(s => !s.FinYear.ToLower().Contains("parent")).FirstOrDefault(s => int.Parse(s.FinYear.Split('_').Last()) == finStatement.FinancialYear);
                        if (statement != null && statement.Content != null)
                        {
                            foreach (var data in statement.Content)
                            {
                                var finData = new Financial
                                {
                                    FinancialStatementId = finStatement.Id,
                                    FinCode = data.Key,
                                    FinValue = data.Value
                                };
                                await UpsertFinancialData(finData);
                                //_db.Financial.Add(finData);
                            }
                        }
                    }
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();  // Commit transaction if all operations succeed
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();  // Rollback if any operation fails
                _logger.LogError(ex, "Failed to update FDE: {Message}.", ex.Message);
                return false;
            }
            return true;
        }

        public async Task<FinancialStatement> UpsertFinancialStatement(FinancialStatement financialStatement)
        {
            var existingStatement = await _db.FinancialStatement.FirstOrDefaultAsync(x => x.EntityId == financialStatement.EntityId && x.FinancialYear == financialStatement.FinancialYear && x.FinancialType == financialStatement.FinancialType);

            if (existingStatement == null)
            {
                // Entity does not exist, so add it
                _db.FinancialStatement.Add(financialStatement);
                return financialStatement;
            }

            return existingStatement;
        }

        public async Task UpsertFinancialData(Financial financial)
        {
            var existingFinancial = await _db.Financial.FirstOrDefaultAsync(x => x.FinancialStatementId == financial.FinancialStatementId && x.FinCode == financial.FinCode);

            if (existingFinancial == null)
            {
                // Entity does not exist, so add it
                _db.Financial.Add(financial);
            }
            else
            {
                // Entity exists, so update it
                //_db.Entry(existingFinancial).CurrentValues.SetValues(financial);
                existingFinancial.FinValue = financial.FinValue;
                _db.Financial.Update(existingFinancial);
            }
        }
    }
}
