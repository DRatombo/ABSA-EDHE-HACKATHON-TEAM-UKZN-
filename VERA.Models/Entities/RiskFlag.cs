using VERA.Models.Enums;
using VERA.Models.Enums.VERA.Models.Enums;

namespace VERA.Models.Entities
{
    public class RiskFlag
    {
        public int RiskFlagId { get; set; }

        public int OpportunityId { get; set; }

        public Opportunity? Opportunity { get; set; }

        public string Category { get; set; } = string.Empty;

        public RiskSeverity Severity { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}