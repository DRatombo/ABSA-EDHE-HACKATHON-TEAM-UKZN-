using VERA.Models.Enums;
using VERA.Models.Enums.VERA.Models.Enums;

namespace VERA.Models.Entities
{
    public class VerificationResult
    {
        public int VerificationResultId { get; set; }

        public int OpportunityId { get; set; }

        public Opportunity? Opportunity { get; set; }

        public string VerificationType { get; set; } = string.Empty;

        public VerificationStatus Status { get; set; }
            = VerificationStatus.Pending;

        public string Evidence { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public bool IsSimulated { get; set; }

        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }
}