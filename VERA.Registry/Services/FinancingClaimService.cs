using Microsoft.EntityFrameworkCore;
using VERA.Registry.Data;
using VERA.Registry.Models;

namespace VERA.Registry.Services
{
    public class FinancingClaimService
    {
        private readonly RegistryDbContext _context;

        public FinancingClaimService(
            RegistryDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasActiveClaimAsync(
            int purchaseOrderId)
        {
            return await _context.FinancingClaims
                .AnyAsync(c =>
                    c.RegisteredPurchaseOrderId ==
                    purchaseOrderId
                    &&
                    c.Status == "ACTIVE");
        }

        public async Task<FinancingClaim> CreateClaimAsync(
            int purchaseOrderId)
        {
            bool activeClaimExists =
                await HasActiveClaimAsync(purchaseOrderId);

            if (activeClaimExists)
            {
                throw new InvalidOperationException(
                    "An active financing claim already exists for this PO.");
            }

            var claim = new FinancingClaim
            {
                RegisteredPurchaseOrderId =
                    purchaseOrderId,

                ClaimReference =
                    $"CLM-{DateTime.UtcNow.Year}-" +
                    Guid.NewGuid()
                        .ToString("N")
                        .Substring(0, 8)
                        .ToUpper(),

                Status = "ACTIVE",

                CreatedAt = DateTime.UtcNow
            };

            _context.FinancingClaims.Add(claim);

            await _context.SaveChangesAsync();

            return claim;
        }

        public async Task ReleaseClaimAsync(
            int claimId)
        {
            var claim =
                await _context.FinancingClaims
                    .FindAsync(claimId);

            if (claim == null)
            {
                throw new InvalidOperationException(
                    "Financing claim not found.");
            }

            claim.Status = "RELEASED";
            claim.ReleasedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}