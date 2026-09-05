using Microsoft.EntityFrameworkCore;
using VERA.Data.Context;
using VERA.Models.Entities;
using VERA.Models.Enums;

namespace VERA.Data.Repositories
{
    public class OpportunityRepository : IOpportunityRepository
    {
        private readonly VeraDbContext _context;

        public OpportunityRepository(VeraDbContext context)
        {
            _context = context;
        }

        public async Task<Opportunity?> GetByIdAsync(int opportunityId, bool includeDetails = false, CancellationToken ct = default)
        {
            IQueryable<Opportunity> query = _context.Opportunities;

            if (includeDetails)
            {
                query = query
                    .Include(o => o.Business)
                    .Include(o => o.VerificationResults)
                    .Include(o => o.RiskFlags)
                    .Include(o => o.FundingOffers)
                    .Include(o => o.FulfilmentRecord);
            }

            return await query.FirstOrDefaultAsync(o => o.OpportunityId == opportunityId, ct);
        }

        public async Task<(IReadOnlyList<Opportunity> Items, int TotalCount)> GetPagedAsync(
            int? businessId,
            OpportunityStatus? status,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            IQueryable<Opportunity> query = _context.Opportunities.AsNoTracking();

            if (businessId is not null)
            {
                query = query.Where(o => o.BusinessId == businessId);
            }

            if (status is not null)
            {
                query = query.Where(o => o.Status == status);
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<bool> DuplicatePoNumberExistsAsync(int businessId, string poNumber, int? excludingOpportunityId, CancellationToken ct = default)
        {
            return await _context.Opportunities.AnyAsync(
                o => o.BusinessId == businessId &&
                     o.PONumber == poNumber &&
                     (excludingOpportunityId == null || o.OpportunityId != excludingOpportunityId),
                ct);
        }

        public async Task<bool> DuplicateFingerprintExistsAsync(string fingerprint, int? excludingOpportunityId, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(fingerprint))
            {
                return false;
            }

            return await _context.Opportunities.AnyAsync(
                o => o.Fingerprint == fingerprint &&
                     (excludingOpportunityId == null || o.OpportunityId != excludingOpportunityId),
                ct);
        }

        public async Task<IReadOnlyList<Opportunity>> GetAllForAssessmentAsync(CancellationToken ct = default)
        {
            return await _context.Opportunities
                .AsNoTracking()
                .Include(o => o.FulfilmentRecord)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Opportunity>> GetByBusinessIdForAssessmentAsync(int businessId, CancellationToken ct = default)
        {
            return await _context.Opportunities
                .AsNoTracking()
                .Include(o => o.FulfilmentRecord)
                .Where(o => o.BusinessId == businessId)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Opportunity opportunity, CancellationToken ct = default)
        {
            await _context.Opportunities.AddAsync(opportunity, ct);
        }

        public void Update(Opportunity opportunity)
        {
            _context.Entry(opportunity).State = EntityState.Modified;
        }

        public async Task<bool> UpdateStatusAsync(int opportunityId, OpportunityStatus newStatus, byte[] expectedRowVersion, CancellationToken ct = default)
        {
            var opportunity = new Opportunity { OpportunityId = opportunityId };
            _context.Attach(opportunity);

            opportunity.Status = newStatus;
            _context.Entry(opportunity).Property(o => o.Status).IsModified = true;

            // Tell EF what RowVersion we *think* is currently in the
            // database. If another request already changed the row, the
            // generated UPDATE's WHERE clause won't match any row, EF
            // detects that as 0 rows affected, and throws
            // DbUpdateConcurrencyException instead of silently overwriting.
            _context.Entry(opportunity).Property(o => o.RowVersion).OriginalValue = expectedRowVersion;

            try
            {
                await _context.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }
    }
}
