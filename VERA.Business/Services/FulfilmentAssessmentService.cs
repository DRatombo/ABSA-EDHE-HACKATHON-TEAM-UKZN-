using VERA.Models.Entities;
using VERA.Models.Enums;
using VERA.Models.Enums.VERA.Models.Enums;

namespace VERA.Business.Services
{
    /// <summary>
    /// Assesses whether an SME appears capable of fulfilling
    /// a particular commercial opportunity.
    ///
    /// The MVP uses transparent rule-based assessment rather than
    /// pretending to have a trained machine-learning risk model.
    /// </summary>
    public class FulfilmentAssessmentService
    {
        /// <summary>
        /// Generates explainable risk flags using the SME's current
        /// opportunity and its previous fulfilment history.
        /// </summary>
        /// <param name="currentOpportunity">
        /// The new opportunity currently being assessed.
        /// </param>
        /// <param name="previousOpportunities">
        /// Previous opportunities belonging to the same SME.
        /// </param>
        /// <returns>
        /// A list of explainable risk flags discovered during assessment.
        /// </returns>
        public List<RiskFlag> Assess(
            Opportunity currentOpportunity,
            IEnumerable<Opportunity> previousOpportunities)
        {
            // Create the list that will contain every risk identified
            // during the assessment.
            List<RiskFlag> riskFlags = new();

            // Only completed opportunities should be considered when
            // assessing the SME's proven historical execution capacity.
            List<Opportunity> completedOpportunities =
                previousOpportunities
                    .Where(o =>
                        o.OpportunityId != currentOpportunity.OpportunityId &&
                        o.Status == OpportunityStatus.Completed)
                    .ToList();

            // ---------------------------------------------------------
            // RULE 1: LIMITED PLATFORM HISTORY
            // ---------------------------------------------------------

            // If VERA has no completed opportunities for this SME,
            // we cannot claim that it has a proven fulfilment history.
            if (completedOpportunities.Count == 0)
            {
                riskFlags.Add(new RiskFlag
                {
                    OpportunityId = currentOpportunity.OpportunityId,
                    Category = "Limited History",
                    Severity = RiskSeverity.Medium,
                    Description =
                        "The SME has no completed opportunities recorded " +
                        "on VERA. Fulfilment capacity cannot yet be confirmed " +
                        "from platform history."
                });
            }

            // ---------------------------------------------------------
            // RULE 2: OPPORTUNITY SCALE
            // ---------------------------------------------------------

            if (completedOpportunities.Count > 0)
            {
                // Find the largest opportunity that this SME has
                // successfully completed through the available history.
                decimal largestPreviousPO =
                    completedOpportunities.Max(o => o.POValue);

                // Avoid division by zero if historical data is incomplete.
                if (largestPreviousPO > 0)
                {
                    decimal scaleMultiple =
                        currentOpportunity.POValue / largestPreviousPO;

                    // If the new PO is more than twice the SME's largest
                    // previously completed opportunity, flag the jump
                    // in execution scale for review.
                    if (scaleMultiple > 2)
                    {
                        riskFlags.Add(new RiskFlag
                        {
                            OpportunityId = currentOpportunity.OpportunityId,
                            Category = "Scale Risk",
                            Severity = RiskSeverity.High,
                            Description =
                                $"Current PO value is {scaleMultiple:F1}x " +
                                "the SME's largest previously completed " +
                                "opportunity."
                        });
                    }
                }
            }

            // ---------------------------------------------------------
            // RULE 3: DELIVERY WINDOW
            // ---------------------------------------------------------

            // Determine how much time the SME has between the PO issue
            // date and required delivery date.
            double deliveryDays =
                (currentOpportunity.DeliveryDate -
                 currentOpportunity.IssueDate).TotalDays;

            // A delivery date before or equal to the issue date represents
            // invalid or suspicious opportunity information.
            if (deliveryDays <= 0)
            {
                riskFlags.Add(new RiskFlag
                {
                    OpportunityId = currentOpportunity.OpportunityId,
                    Category = "Delivery Window",
                    Severity = RiskSeverity.Critical,
                    Description =
                        "The required delivery date is not after the " +
                        "purchase order issue date."
                });
            }
            // A very short fulfilment window may create execution risk.
            // For the MVP, less than seven days is flagged for review.
            else if (deliveryDays < 7)
            {
                riskFlags.Add(new RiskFlag
                {
                    OpportunityId = currentOpportunity.OpportunityId,
                    Category = "Delivery Window",
                    Severity = RiskSeverity.High,
                    Description =
                        $"The opportunity allows only " +
                        $"{Math.Ceiling(deliveryDays)} days for fulfilment."
                });
            }

            // ---------------------------------------------------------
            // RULE 4: SME CONTRIBUTION
            // ---------------------------------------------------------

            // An SME contribution is not automatically required for every
            // finance product. However, zero contribution is useful context
            // for a funder because the entire fulfilment cost requires
            // external capital.
            if (currentOpportunity.SMEContribution <= 0 &&
                currentOpportunity.FulfilmentCost > 0)
            {
                riskFlags.Add(new RiskFlag
                {
                    OpportunityId = currentOpportunity.OpportunityId,
                    Category = "Funding Dependence",
                    Severity = RiskSeverity.Medium,
                    Description =
                        "The SME currently requires external capital for " +
                        "the full estimated fulfilment cost."
                });
            }

            // Return all identified risks.
            return riskFlags;
        }
    }
}