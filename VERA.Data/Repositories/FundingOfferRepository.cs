using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using VERA.Data.Context;
using VERA.Models.Entities;
using VERA.Models.Enums;

namespace VERA.Data.Repositories
{
    public class FundingOfferRepository : IFundingOfferRepository
    {
        private readonly VeraDbContext _context;

        public FundingOfferRepository(VeraDbContext context)
        {
            _context = context;
        }

        public async Task<FundingOffer?> GetByIdAsync(int fundingOfferId, CancellationToken ct = default)
        {
            return await _context.FundingOffers.FirstOrDefaultAsync(f => f.FundingOfferId == fundingOfferId, ct);
        }

        public async Task<IReadOnlyList<FundingOffer>> GetByOpportunityIdAsync(int opportunityId, CancellationToken ct = default)
        {
            return await _context.FundingOffers
                .AsNoTracking()
                .Where(f => f.OpportunityId == opportunityId)
                .OrderBy(f => f.Amount)
                .ToListAsync(ct);
        }

        public async Task AddAsync(FundingOffer offer, CancellationToken ct = default)
        {
            await _context.FundingOffers.AddAsync(offer, ct);
        }

        public void Update(FundingOffer offer)
        {
            _context.Entry(offer).State = EntityState.Modified;
        }

        public async Task<AcceptOfferResult> AcceptOfferAsync(int fundingOfferId, CancellationToken ct = default)
        {
            // CreateExecutionStrategy is required here because the DbContext
            // is configured with EnableRetryOnFailure - SQL Server can retry
            // a failed transient operation, but only if the whole
            // begin/commit block is wrapped through the execution strategy.
            // Doing a bare BeginTransactionAsync alongside retry-on-failure
            // would throw at runtime.
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(ct);

                var offer = await _context.FundingOffers
                    .FirstOrDefaultAsync(f => f.FundingOfferId == fundingOfferId, ct);

                if (offer is null)
                {
                    return AcceptOfferResult.NotFound;
                }

                var alreadyAccepted = await _context.FundingOffers
                    .AnyAsync(f => f.OpportunityId == offer.OpportunityId && f.IsAccepted, ct);

                if (alreadyAccepted)
                {
                    // Someone already accepted an offer on this opportunity -
                    // whether it's this one or a different one, we refuse to
                    // proceed rather than fund the same PO twice.
                    return AcceptOfferResult.AlreadyDecided;
                }

                offer.IsAccepted = true;

                var opportunity = await _context.Opportunities
                    .FirstOrDefaultAsync(o => o.OpportunityId == offer.OpportunityId, ct);

                if (opportunity is not null)
                {
                    opportunity.Status = OpportunityStatus.Funded;
                }

                try
                {
                    await _context.SaveChangesAsync(ct);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Another request touched the offer or the opportunity
                    // between our read and our write - roll back and make the
                    // caller re-fetch and retry instead of guessing.
                    await transaction.RollbackAsync(ct);
                    return AcceptOfferResult.ConcurrencyConflict;
                }

                await transaction.CommitAsync(ct);
                return AcceptOfferResult.Accepted;

            });
        }
    }
}

