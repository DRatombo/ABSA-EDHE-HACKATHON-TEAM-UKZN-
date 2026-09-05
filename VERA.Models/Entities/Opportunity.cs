using System.ComponentModel.DataAnnotations;
using VERA.Models.Enums;

namespace VERA.Models.Entities
{
    public class Opportunity
    {
        public int OpportunityId { get; set; }

        public int BusinessId { get; set; }

        public Business? Business { get; set; }

        [Required]
        public string BuyerName { get; set; } = string.Empty;

        public string BuyerReference { get; set; } = string.Empty;

        [Required]
        public string PONumber { get; set; } = string.Empty;

        public decimal POValue { get; set; }

        public decimal FulfilmentCost { get; set; }

        public decimal SMEContribution { get; set; }

        public decimal FundingGap { get; set; }

        public decimal EstimatedFundingCost { get; set; }

        public decimal PlatformFee { get; set; }

        public decimal RemainingMargin { get; set; }

        public decimal RemainingMarginPercentage { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime DeliveryDate { get; set; }

        public OpportunityStatus Status { get; set; }
            = OpportunityStatus.Draft;

        public string Fingerprint { get; set; } = string.Empty;

        public string? UploadedPOFileName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<VerificationResult> VerificationResults { get; set; }
            = new();

        public List<RiskFlag> RiskFlags { get; set; }
            = new();

        public List<FundingOffer> FundingOffers { get; set; }
            = new();

        public FulfilmentRecord? FulfilmentRecord { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}