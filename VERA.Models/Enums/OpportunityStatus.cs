using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VERA.Models.Enums
{
    public enum OpportunityStatus
    {
        Draft,
        Submitted,
        VerificationPending,
        VerificationComplete,
        ManualReview,
        FinanceReady,
        FundingReview,
        FundingOffered,
        Funded,
        InFulfilment,
        Delivered,
        BuyerPaid,
        Settled,
        Completed,
        Blocked
    }
}