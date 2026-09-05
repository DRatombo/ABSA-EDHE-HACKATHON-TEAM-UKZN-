using Microsoft.EntityFrameworkCore;
using VERA.Registry.Data;
using VERA.Registry.Models.ViewModels;

namespace VERA.Registry.Services
{
    public class RegistryVerificationService
    {
        private readonly RegistryDbContext _context;

        public RegistryVerificationService(
            RegistryDbContext context)
        {
            _context = context;
        }

        public async Task<VerifyPOResult> VerifyAsync(
            VerifyPORequest request)
        {
            var result = new VerifyPOResult();

            var po = await _context.RegisteredPurchaseOrders
                .Include(p => p.RegistryIssuer)
                .Include(p => p.FinancingClaims)
                .FirstOrDefaultAsync(p =>
                    p.VeraPOId == request.VeraPOId);

            if (po == null)
            {
                result.RecordFound = false;
                result.Result = "REVIEW";

                result.ReasonCodes.Add(
                    "REG-001 PO_NOT_FOUND");

                return result;
            }

            result.RecordFound = true;
            result.PurchaseOrder = po;

            result.PONumberMatch =
                string.Equals(
                    po.PONumber.Trim(),
                    request.PONumber.Trim(),
                    StringComparison.OrdinalIgnoreCase);

            result.SupplierMatch =
                string.Equals(
                    po.SupplierName.Trim(),
                    request.SupplierName.Trim(),
                    StringComparison.OrdinalIgnoreCase);

            result.AmountMatch =
                po.POValue == request.Amount;

            result.IsActive =
                string.Equals(
                    po.Status,
                    "ACTIVE",
                    StringComparison.OrdinalIgnoreCase);

            result.HasActiveFinancingClaim =
                po.FinancingClaims.Any(c =>
                    c.Status == "ACTIVE");

            if (!result.PONumberMatch)
            {
                result.ReasonCodes.Add(
                    "REG-002 PO_NUMBER_MISMATCH");
            }

            if (!result.SupplierMatch)
            {
                result.ReasonCodes.Add(
                    "REG-003 SUPPLIER_MISMATCH");
            }

            if (!result.AmountMatch)
            {
                result.ReasonCodes.Add(
                    "REG-004 AMOUNT_MISMATCH");
            }

            if (!result.IsActive)
            {
                result.ReasonCodes.Add(
                    "REG-005 PO_NOT_ACTIVE");
            }

            if (result.HasActiveFinancingClaim)
            {
                result.ReasonCodes.Add(
                    "REG-006 ACTIVE_FINANCING_CLAIM");
            }

            if (!result.AmountMatch
    || !result.IsActive
    || result.HasActiveFinancingClaim)
            {
                result.Result = "BLOCK";
            }
            else if (!result.PONumberMatch
                     || !result.SupplierMatch)
            {
                result.Result = "REVIEW";
            }
            else
            {
                result.Result = "PASS";
            }

            return result;
        }
    }
}