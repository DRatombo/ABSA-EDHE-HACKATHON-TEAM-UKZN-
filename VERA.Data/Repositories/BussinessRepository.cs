using Microsoft.EntityFrameworkCore;
using VERA.Data.Context;
using VERA.Models.Entities;

namespace VERA.Data.Repositories
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly VeraDbContext _context;

        public BusinessRepository(VeraDbContext context)
        {
            _context = context;
        }

        public async Task<Business?> GetByIdAsync(int businessId, bool includeOpportunities = false, CancellationToken ct = default)
        {
            IQueryable<Business> query = _context.Businesses;

            if (includeOpportunities)
            {
                query = query.Include(b => b.Opportunities);
            }

            // AsNoTracking would be cheaper for read-only callers, but we
            // deliberately keep this entity tracked because most callers of
            // GetByIdAsync go on to call Update() in the same unit of work.
            return await query.FirstOrDefaultAsync(b => b.BusinessId == businessId, ct);
        }

        public async Task<Business?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken ct = default)
        {
            // EF Core parameterizes this automatically - registrationNumber
            // is never concatenated into SQL, so this is safe from injection
            // regardless of what the caller passes in.
            return await _context.Businesses
                .FirstOrDefaultAsync(b => b.RegistrationNumber == registrationNumber, ct);
        }

        public async Task<(IReadOnlyList<Business> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100); // hard cap - no client can ask for 1,000,000 rows

            IQueryable<Business> query = _context.Businesses.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(b =>
                    EF.Functions.Like(b.BusinessName, $"%{term}%") ||
                    EF.Functions.Like(b.RegistrationNumber, $"%{term}%"));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(b => b.BusinessName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<bool> RegistrationNumberInUseAsync(string registrationNumber, int? excludingBusinessId, CancellationToken ct = default)
        {
            return await _context.Businesses.AnyAsync(
                b => b.RegistrationNumber == registrationNumber &&
                     (excludingBusinessId == null || b.BusinessId != excludingBusinessId),
                ct);
        }

        public async Task AddAsync(Business business, CancellationToken ct = default)
        {
            await _context.Businesses.AddAsync(business, ct);
        }

        public void Update(Business business)
        {
            _context.Entry(business).State = EntityState.Modified;
        }

        public async Task<bool> DeleteAsync(int businessId, CancellationToken ct = default)
        {
            var business = await _context.Businesses
                .Include(b => b.Opportunities)
                .FirstOrDefaultAsync(b => b.BusinessId == businessId, ct);

            if (business is null)
            {
                return false;
            }

            if (business.Opportunities.Count > 0)
            {
                // Deliberately refuse in application code too, not just rely
                // on the DB FK throwing - gives callers a clean bool instead
                // of an unhandled DbUpdateException.
                return false;
            }

            _context.Businesses.Remove(business);
            return true;
        }
    }
}
