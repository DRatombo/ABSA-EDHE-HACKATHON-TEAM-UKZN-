using VERA.Models.Entities;
using VERA.Models.Enums;

// Alias used to avoid a naming conflict between the
// VERA.Business namespace and the Business entity.
using BusinessEntity = VERA.Models.Entities.Business;

namespace VERA.Business.Services
{
    // Builds an SME's Fulfilment Passport using completed opportunities and their actual fulfilment outcomes.
    // The passport gives funders a transparent view of the SME's demonstrated execution history over time.
    public class FulfilmentPassportService
    { 
        // Calculates key performance indicators for an SME using completed opportunities stored in VERA.
        // The SME for which the passport is being generated.
        // All opportunities available for the SME.
        // A FulfilmentPassportSummary containing historical performance metrics is returned 
        public FulfilmentPassportSummary Build(
            BusinessEntity business,
            IEnumerable<Opportunity> opportunities)
        {
            // ---------------------------------------------------------
            // 1. GET COMPLETED OPPORTUNITIES
            // ---------------------------------------------------------
            //
            // Only completed opportunities count toward the SME's
            // proven fulfilment history.
            List<Opportunity> completedOpportunities =
                opportunities
                    .Where(o =>
                        o.BusinessId == business.BusinessId &&
                        o.Status == OpportunityStatus.Completed)
                    .ToList();

            // ---------------------------------------------------------
            // 2. TOTAL COMPLETED VALUE
            // ---------------------------------------------------------
            //
            // Add together the value of every completed opportunity.
            //
            // Example:
            // PO 1 = R50,000
            // PO 2 = R70,000
            // PO 3 = R100,000
            //
            // Total Completed Value = R220,000
            decimal totalValue =
                completedOpportunities.Sum(o => o.POValue);

            // ---------------------------------------------------------
            // 3. LARGEST COMPLETED OPPORTUNITY
            // ---------------------------------------------------------
            //
            // This is useful when assessing whether a new opportunity
            // represents a significant jump in execution scale.
            decimal largestPO =
                completedOpportunities.Count > 0
                    ? completedOpportunities.Max(o => o.POValue)
                    : 0;

            // ---------------------------------------------------------
            // 4. AVERAGE COMPLETED OPPORTUNITY
            // ---------------------------------------------------------
            //
            // This provides additional context about the typical size
            // of opportunities successfully completed by the SME.
            decimal averagePO =
                completedOpportunities.Count > 0
                    ? completedOpportunities.Average(o => o.POValue)
                    : 0;

            // ---------------------------------------------------------
            // 5. GET OPPORTUNITIES WITH ACTUAL OUTCOME DATA
            // ---------------------------------------------------------
            //
            // Some completed opportunities may not yet contain a
            // FulfilmentRecord.
            //
            // Only opportunities with outcome data can be used to
            // calculate delivery performance, settlement rates
            // and disputes.
            List<Opportunity> opportunitiesWithOutcomes =
                completedOpportunities
                    .Where(o => o.FulfilmentRecord != null)
                    .ToList();

            // ---------------------------------------------------------
            // 6. ON-TIME DELIVERY RATE
            // ---------------------------------------------------------

            double onTimeRate = 0;

            // Only calculate the percentage when actual outcome
            // records are available.
            if (opportunitiesWithOutcomes.Count > 0)
            {
                // Count how many completed opportunities were
                // delivered on or before the agreed delivery date.
                int onTimeCount =
                    opportunitiesWithOutcomes.Count(o =>
                        o.FulfilmentRecord!.DeliveredOnTime);

                // Convert the result into a percentage.
                onTimeRate =
                    (double)onTimeCount
                    / opportunitiesWithOutcomes.Count
                    * 100;
            }

            // ---------------------------------------------------------
            // 7. FUNDER SETTLEMENT RATE
            // ---------------------------------------------------------
            //
            // This shows the percentage of completed opportunities
            // where the funding provider was successfully settled.
            double settlementRate = 0;

            if (opportunitiesWithOutcomes.Count > 0)
            {
                // Count opportunities where the funder was settled.
                int settledCount =
                    opportunitiesWithOutcomes.Count(o =>
                        o.FulfilmentRecord!.FunderSettled);

                // Convert the result into a percentage.
                settlementRate =
                    (double)settledCount
                    / opportunitiesWithOutcomes.Count
                    * 100;
            }

            // ---------------------------------------------------------
            // 8. DISPUTE COUNT
            // ---------------------------------------------------------
            //
            // Count completed opportunities where a dispute occurred.
            //
            // A dispute does not automatically mean failure,
            // but it gives funders additional historical context.
            int disputeCount =
                opportunitiesWithOutcomes.Count(o =>
                    o.FulfilmentRecord!.DisputeOccurred);

            // ---------------------------------------------------------
            // 9. SUCCESSFUL BUYER ACCEPTANCE RATE
            // ---------------------------------------------------------
            //
            // This measures how often completed deliveries were accepted
            // by the buyer.
            double buyerAcceptanceRate = 0;

            if (opportunitiesWithOutcomes.Count > 0)
            {
                int acceptedCount =
                    opportunitiesWithOutcomes.Count(o =>
                        o.FulfilmentRecord!.BuyerAcceptedDelivery);

                buyerAcceptanceRate =
                    (double)acceptedCount
                    / opportunitiesWithOutcomes.Count
                    * 100;
            }

            // ---------------------------------------------------------
            // 10. BUILD THE PASSPORT SUMMARY
            // ---------------------------------------------------------
            //
            // Return all calculated performance information in one object
            // that can later be displayed on the VERA Fulfilment Passport.
            return new FulfilmentPassportSummary
            {
                BusinessId = business.BusinessId,

                BusinessName = business.BusinessName,

                CompletedPOs = completedOpportunities.Count,

                TotalCompletedValue = totalValue,

                LargestCompletedPO = largestPO,

                AverageCompletedPO = averagePO,

                OnTimeDeliveryRate = onTimeRate,

                BuyerAcceptanceRate = buyerAcceptanceRate,

                FunderSettlementRate = settlementRate,

                DisputeCount = disputeCount
            };
        }
    }

    /// <summary>
    /// Represents the calculated Fulfilment Passport metrics
    /// for an SME.
    ///
    /// This object is returned by FulfilmentPassportService
    /// and can later be displayed in the Web layer.
    /// </summary>
    public class FulfilmentPassportSummary
    {
        /// <summary>
        /// Unique identifier of the SME.
        /// </summary>
        public int BusinessId { get; set; }

        /// <summary>
        /// Name of the SME.
        /// </summary>
        public string BusinessName { get; set; } = string.Empty;

        /// <summary>
        /// Number of opportunities successfully completed.
        /// </summary>
        public int CompletedPOs { get; set; }

        /// <summary>
        /// Total commercial value of completed opportunities.
        /// </summary>
        public decimal TotalCompletedValue { get; set; }

        /// <summary>
        /// Largest opportunity successfully completed.
        /// </summary>
        public decimal LargestCompletedPO { get; set; }

        /// <summary>
        /// Average value of completed opportunities.
        /// </summary>
        public decimal AverageCompletedPO { get; set; }

        /// <summary>
        /// Percentage of completed opportunities delivered on time.
        /// </summary>
        public double OnTimeDeliveryRate { get; set; }

        /// <summary>
        /// Percentage of deliveries accepted by buyers.
        /// </summary>
        public double BuyerAcceptanceRate { get; set; }

        /// <summary>
        /// Percentage of completed opportunities where funders
        /// were successfully settled.
        /// </summary>
        public double FunderSettlementRate { get; set; }

        /// <summary>
        /// Number of completed opportunities where a dispute occurred.
        /// </summary>
        public int DisputeCount { get; set; }
    }
}