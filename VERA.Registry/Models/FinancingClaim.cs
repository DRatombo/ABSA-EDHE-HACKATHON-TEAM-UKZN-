using System.ComponentModel.DataAnnotations;

namespace VERA.Registry.Models
{
    public class FinancingClaim
    {
        public int FinancingClaimId { get; set; }

        [Required]
        public int RegisteredPurchaseOrderId { get; set; }

        public RegisteredPurchaseOrder? RegisteredPurchaseOrder { get; set; }

        [Required]
        public string ClaimReference { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "ACTIVE";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReleasedAt { get; set; }
    }
}