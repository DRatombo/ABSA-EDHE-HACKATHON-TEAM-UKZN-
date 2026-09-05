using VERA.Models.Entities;

namespace VERA.Business.Services
{
    /// <summary>
    /// Handles the financial calculations for an SME opportunity.
    /// This service determines how much external funding the SME needs
    /// and whether the opportunity remains financially viable after costs.
    /// </summary>
    public class FundingCalculatorService
    {
        /// <summary>
        /// Calculates the funding gap and estimated remaining SME margin
        /// for a specific opportunity.
        /// </summary>
        /// <param name="opportunity">
        /// The opportunity containing the PO value, fulfilment costs,
        /// SME contribution and estimated funding-related costs.
        /// </param>
        public void Calculate(Opportunity opportunity)
        {
            // The funding gap represents the amount of external capital
            // required after considering what the SME can contribute itself.
            //
            // Example:
            // Fulfilment Cost = R100,000
            // SME Contribution = R20,000
            // Funding Gap = R80,000
            opportunity.FundingGap = Math.Max(
                0,
                opportunity.FulfilmentCost - opportunity.SMEContribution
            );

            // Calculate how much value remains for the SME after paying:
            // 1. The cost of fulfilling the purchase order
            // 2. The estimated cost of external funding
            // 3. The VERA platform fee
            //
            // This helps prevent an SME from accepting funding that makes
            // the underlying opportunity commercially unattractive.
            opportunity.RemainingMargin =
                opportunity.POValue
                - opportunity.FulfilmentCost
                - opportunity.EstimatedFundingCost
                - opportunity.PlatformFee;

            // Calculate the remaining margin as a percentage of the PO value.
            // This makes it easier to compare opportunities of different sizes.
            if (opportunity.POValue > 0)
            {
                opportunity.RemainingMarginPercentage =
                    (opportunity.RemainingMargin / opportunity.POValue) * 100;
            }
            else
            {
                // Avoid division by zero if an invalid PO value was provided.
                opportunity.RemainingMarginPercentage = 0;
            }
        }
    }
}