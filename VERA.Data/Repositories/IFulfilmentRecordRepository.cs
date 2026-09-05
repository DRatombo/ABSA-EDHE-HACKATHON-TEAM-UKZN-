using VERA.Models.Entities;

namespace VERA.Data.Repositories
{
    public interface IFulfilmentRecordRepository
    {
        Task<FulfilmentRecord?> GetByOpportunityIdAsync(int opportunityId, CancellationToken ct = default);

        /// <summary>
        /// Creates the record if one doesn't exist for this opportunity yet,
        /// otherwise updates the existing one. Opportunity 1---0/1
        /// FulfilmentRecord, so this avoids callers accidentally creating a
        /// second row and hitting the unique FK index.
        /// </summary>
        Task<FulfilmentRecord> UpsertAsync(FulfilmentRecord record, CancellationToken ct = default);
    }
}
