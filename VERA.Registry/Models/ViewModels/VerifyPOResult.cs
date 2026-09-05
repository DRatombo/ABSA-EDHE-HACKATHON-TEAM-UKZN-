using VERA.Registry.Models;

namespace VERA.Registry.Models.ViewModels
{
    public class VerifyPOResult
    {
        public bool RecordFound { get; set; }

        public bool PONumberMatch { get; set; }

        public bool SupplierMatch { get; set; }

        public bool AmountMatch { get; set; }

        public bool IsActive { get; set; }

        public bool HasActiveFinancingClaim { get; set; }

        public string Result { get; set; } = "REVIEW";

        public List<string> ReasonCodes { get; set; }
            = new List<string>();

        public RegisteredPurchaseOrder? PurchaseOrder { get; set; }
    }
}