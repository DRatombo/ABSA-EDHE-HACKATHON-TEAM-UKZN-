using System.ComponentModel.DataAnnotations;

namespace VERA.Models.Entities
{
    public class FundingOffer
    {
        public int FundingOfferId { get; set; }

        public int OpportunityId { get; set; }

        public Opportunity? Opportunity { get; set; }

        public string FunderName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public decimal FundingCost { get; set; }

        public int TermDays { get; set; }

        public string Notes { get; set; } = string.Empty;

        public bool IsIllustrative { get; set; } = true;

        public bool IsAccepted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}