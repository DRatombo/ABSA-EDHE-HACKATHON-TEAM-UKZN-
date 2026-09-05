using System.ComponentModel.DataAnnotations;

namespace VERA.Models.Entities
{
    public class FulfilmentRecord
    {
        public int FulfilmentRecordId { get; set; }

        public int OpportunityId { get; set; }

        public Opportunity? Opportunity { get; set; }

        public DateTime? FundedDate { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        public bool DeliveredOnTime { get; set; }

        public bool BuyerAcceptedDelivery { get; set; }

        public DateTime? BuyerPaidDate { get; set; }

        public bool FunderSettled { get; set; }

        public bool DisputeOccurred { get; set; }

        public string Outcome { get; set; } = string.Empty;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
     
    }