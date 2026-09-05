using VERA.Models.Entities;
using VERA.Models.Enums;

namespace VERA.Data.Repositories
{
    public interface IOpportunityRepository
    {
        Task<Opportunity?> GetByIdAsync(int opportunityId, bool includeDetails = false, CancellationToken ct = default);

        Task<(IReadOnlyList<Opportunity> Items, int TotalCount)> GetPagedAsync(
            int? businessId,
            OpportunityStatus? status,
            int page,
            int pageSize,
            CancellationToken ct = default);

        /// <summary>
        /// True if this business already has an opportunity open against
        /// this exact PO number (see the unique index in VeraDbContext -
        /// this is the friendly pre-check, the index is the real guard).
        /// </summary>
        Task<bool> DuplicatePoNumberExistsAsync(int businessId, string poNumber, int? excludingOpportunityId, CancellationToken ct = default);

        /// <summary>
        /// True if any opportunity (for any business) already has this
        /// document fingerprint - catches the same PO being used to open
        /// two opportunities, including across different businesses.
        /// </summary>
        Task<bool> DuplicateFingerprintExistsAsync(string fingerprint, int? excludingOpportunityId, CancellationToken ct = default);

        /// <summary>
        /// All opportunities platform-wide, untracked, with just enough data
        /// loaded (Fingerprint, FulfilmentRecord) for
        /// VERA.Business.Services.DuplicateDetectionService to run its
        /// checks. Deliberately unpaged - the assessment engine needs to see
        /// every fingerprint that exists, not a page of them, or a
        /// duplicate PO could slip through simply by landing on a later
        /// page. Fine at hackathon/demo scale; if this table grows large,
        /// switch DuplicateDetectionService to a targeted
        /// DuplicateFingerprintExistsAsync lookup instead of loading
        /// everything into memory.
        /// </summary>
        Task<IReadOnlyList<Opportunity>> GetAllForAssessmentAsync(CancellationToken ct = default);

        /// <summary>
        /// A single business's opportunities, untracked, with
        /// FulfilmentRecord loaded - the history
        /// VERA.Business.Services.FulfilmentAssessmentService and
        /// FulfilmentPassportService need to score execution risk and build
        /// the Fulfilment Passport.
        /// </summary>
        Task<IReadOnlyList<Opportunity>> GetByBusinessIdForAssessmentAsync(int businessId, CancellationToken ct = default);

        Task AddAsync(Opportunity opportunity, CancellationToken ct = default);

        void Update(Opportunity opportunity);

        /// <summary>
        /// Updates only the Status column using the row's current
        /// RowVersion as a concurrency check. Throws
        /// DbUpdateConcurrencyException if another request changed the row
        /// first - callers should treat that as "reload and try again",
        /// never as "force it through".
        /// </summary>
        Task<bool> UpdateStatusAsync(int opportunityId, OpportunityStatus newStatus, byte[] expectedRowVersion, CancellationToken ct = default);
    }
}
