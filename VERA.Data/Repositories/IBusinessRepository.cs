using VERA.Models.Entities;

namespace VERA.Data.Repositories
{
    public interface IBusinessRepository
    {
        Task<Business?> GetByIdAsync(int businessId, bool includeOpportunities = false, CancellationToken ct = default);

        Task<Business?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken ct = default);

        /// <summary>
        /// Paged, optionally-filtered list. Always paged so a caller can never
        /// accidentally (or maliciously) trigger an unbounded "select *"
        /// against the whole Businesses table.
        /// </summary>
        Task<(IReadOnlyList<Business> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            int page,
            int pageSize,
            CancellationToken ct = default);

        /// <summary>
        /// True if a *different* business already owns this registration
        /// number. Used to give a friendly validation error before hitting
        /// the database's unique-index constraint.
        /// </summary>
        Task<bool> RegistrationNumberInUseAsync(string registrationNumber, int? excludingBusinessId, CancellationToken ct = default);

        Task AddAsync(Business business, CancellationToken ct = default);

        void Update(Business business);

        /// <summary>
        /// Returns false (and does nothing) if the business has any
        /// opportunities - the FK is configured with DeleteBehavior.Restrict
        /// on purpose, so a business with financial history can never be
        /// silently removed along with its audit trail.
        /// </summary>
        Task<bool> DeleteAsync(int businessId, CancellationToken ct = default);
    }
}
