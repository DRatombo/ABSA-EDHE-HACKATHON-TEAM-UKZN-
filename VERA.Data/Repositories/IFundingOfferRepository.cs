using System.ComponentModel.DataAnnotations;
using VERA.Models.Entities;

namespace VERA.Data.Repositories
{
    public enum AcceptOfferResult
    {
        Accepted,
        NotFound,
        AlreadyDecided,
        ConcurrencyConflict
    }

    public interface IFundingOfferRepository
    {
        Task<FundingOffer?> GetByIdAsync(int fundingOfferId, CancellationToken ct = default);

        Task<IReadOnlyList<FundingOffer>> GetByOpportunityIdAsync(int opportunityId, CancellationToken ct = default);

        Task AddAsync(FundingOffer offer, CancellationToken ct = default);

        void Update(FundingOffer offer);

        /// <summary>
        /// Accepts one funding offer for an opportunity and rejects every
        /// other pending offer on that same opportunity, all inside a single
        /// database transaction. This is the operation that stops the same
        /// PO being funded twice: either every write in here lands, or none
        /// of them do.
        /// </summary>
        Task<AcceptOfferResult> AcceptOfferAsync(int fundingOfferId, CancellationToken ct = default);
    }
}
