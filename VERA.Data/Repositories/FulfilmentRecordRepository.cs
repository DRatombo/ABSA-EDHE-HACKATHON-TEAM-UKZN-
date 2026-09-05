using Microsoft.EntityFrameworkCore;
using VERA.Data.Context;
using VERA.Models.Entities;

namespace VERA.Data.Repositories
{
    public class FulfilmentRecordRepository : IFulfilmentRecordRepository
    {
        private readonly VeraDbContext _context;

        public FulfilmentRecordRepository(VeraDbContext context)
        {
            _context = context;
        }

        public async Task<FulfilmentRecord?> GetByOpportunityIdAsync(int opportunityId, CancellationToken ct = default)
        {
            return await _context.FulfilmentRecords
                .FirstOrDefaultAsync(f => f.OpportunityId == opportunityId, ct);
        }

        public async Task<FulfilmentRecord> UpsertAsync(FulfilmentRecord record, CancellationToken ct = default)
        {
            var existing = await _context.FulfilmentRecords
                .FirstOrDefaultAsync(f => f.OpportunityId == record.OpportunityId, ct);

            if (existing is null)
            {
                await _context.FulfilmentRecords.AddAsync(record, ct);
                return record;
            }

            existing.FundedDate = record.FundedDate;
            existing.ActualDeliveryDate = record.ActualDeliveryDate;
            existing.DeliveredOnTime = record.DeliveredOnTime;
            existing.BuyerAcceptedDelivery = record.BuyerAcceptedDelivery;
            existing.BuyerPaidDate = record.BuyerPaidDate;
            existing.FunderSettled = record.FunderSettled;
            existing.DisputeOccurred = record.DisputeOccurred;
            existing.Outcome = record.Outcome;

            return existing;
        }
    }
}
