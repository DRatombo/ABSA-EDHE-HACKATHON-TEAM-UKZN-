using VERA.Models.Entities;
using VERA.Models.Enums;
using VERA.Models.Enums.VERA.Models.Enums;

namespace VERA.Business.Services
{
    /// <summary>
    /// Creates the verification profile for a commercial opportunity.
    ///
    /// In the hackathon MVP, some external checks are simulated because
    /// VERA does not have live production access to KYC, CIPC, buyer,
    /// supplier, banking or funder APIs.
    ///
    /// The purpose of this service is to keep those checks transparent
    /// and explainable rather than pretending they are live integrations.
    /// </summary>
    public class VerificationService
    {
        /// <summary>
        /// Runs the MVP verification process for an opportunity.
        /// </summary>
        /// <param name="opportunity">
        /// The opportunity currently being verified.
        /// </param>
        /// <param name="isDuplicate">
        /// Indicates whether VERA's internal duplicate detection found
        /// a matching opportunity fingerprint.
        /// </param>
        /// <returns>
        /// A list of verification results that can be displayed to
        /// both the SME and the funder.
        /// </returns>
        public List<VerificationResult> Verify(
            Opportunity opportunity,
            bool isDuplicate)
        {
            // Create a new collection to store every verification result.
            List<VerificationResult> results = new();

            // ---------------------------------------------------------
            // 1. IDENTITY VERIFICATION
            // ---------------------------------------------------------
            //
            // This represents a future integration with an authorised
            // KYC and liveness provider.
            //
            // It is simulated in the MVP and clearly labelled as such.
            results.Add(new VerificationResult
            {
                OpportunityId = opportunity.OpportunityId,

                VerificationType = "Identity Verification",

                Status = VerificationStatus.Verified,

                Evidence =
                    "Business representative identity and liveness " +
                    "check passed for the demo scenario.",

                Source =
                    "Simulated authorised KYC provider",

                IsSimulated = true,

                CheckedAt = DateTime.UtcNow
            });

            // ---------------------------------------------------------
            // 2. BUSINESS LEGITIMACY
            // ---------------------------------------------------------
            //
            // In production this could use authorised company
            // registration and business-information sources.
            results.Add(new VerificationResult
            {
                OpportunityId = opportunity.OpportunityId,

                VerificationType = "Business Legitimacy",

                Status = VerificationStatus.Verified,

                Evidence =
                    "Business registration details matched the " +
                    "submitted SME profile in the demo scenario.",

                Source =
                    "Simulated business verification adapter",

                IsSimulated = true,

                CheckedAt = DateTime.UtcNow
            });

            // ---------------------------------------------------------
            // 3. BUYER VERIFICATION
            // ---------------------------------------------------------
            //
            // VERA should independently verify that the buyer exists
            // and that the buyer information aligns with the PO.
            results.Add(new VerificationResult
            {
                OpportunityId = opportunity.OpportunityId,

                VerificationType = "Buyer Verification",

                Status = VerificationStatus.Verified,

                Evidence =
                    $"Buyer '{opportunity.BuyerName}' was confirmed " +
                    "for the demo opportunity.",

                Source =
                    "Simulated buyer confirmation",

                IsSimulated = true,

                CheckedAt = DateTime.UtcNow
            });

            // ---------------------------------------------------------
            // 4. PURCHASE ORDER VERIFICATION
            // ---------------------------------------------------------
            //
            // This confirms that the opportunity contains the basic
            // commercial information required by VERA.
            //
            // This is partly real internal logic because VERA can
            // validate whether the extracted PO information is complete.
            VerificationStatus poStatus =
                IsPODataComplete(opportunity)
                    ? VerificationStatus.Verified
                    : VerificationStatus.Flagged;

            results.Add(new VerificationResult
            {
                OpportunityId = opportunity.OpportunityId,

                VerificationType = "Purchase Order Verification",

                Status = poStatus,

                Evidence =
                    poStatus == VerificationStatus.Verified
                        ? "Required PO fields are present and structurally valid."
                        : "One or more required PO fields are incomplete.",

                Source =
                    "VERA internal validation",

                IsSimulated = false,

                CheckedAt = DateTime.UtcNow
            });

            // ---------------------------------------------------------
            // 5. DUPLICATE FINANCING CHECK
            // ---------------------------------------------------------
            //
            // This is a real MVP check against opportunities already
            // stored inside VERA.
            //
            // Important:
            // This does NOT claim cross-bank or cross-lender coverage.
            results.Add(new VerificationResult
            {
                OpportunityId = opportunity.OpportunityId,

                VerificationType = "Duplicate Financing Check",

                Status =
                    isDuplicate
                        ? VerificationStatus.Flagged
                        : VerificationStatus.Verified,

                Evidence =
                    isDuplicate
                        ? "A matching opportunity fingerprint exists " +
                          "in VERA's internal opportunity registry."
                        : "No matching opportunity fingerprint was found " +
                          "in VERA's internal opportunity registry.",

                Source =
                    "VERA internal opportunity registry",

                IsSimulated = false,

                CheckedAt = DateTime.UtcNow
            });

            // ---------------------------------------------------------
            // 6. SUPPLIER READINESS
            // ---------------------------------------------------------
            //
            // Supplier confirmation is simulated for the hackathon MVP.
            results.Add(new VerificationResult
            {
                OpportunityId = opportunity.OpportunityId,

                VerificationType = "Supplier Readiness",

                Status = VerificationStatus.Verified,

                Evidence =
                    "Supplier quotations and availability were confirmed " +
                    "for the demo scenario.",

                Source =
                    "Simulated supplier confirmation",

                IsSimulated = true,

                CheckedAt = DateTime.UtcNow
            });

            // Return the complete explainable verification profile.
            return results;
        }

        /// <summary>
        /// Checks whether the opportunity contains the minimum PO data
        /// required for VERA's internal verification workflow.
        /// </summary>
        private bool IsPODataComplete(Opportunity opportunity)
        {
            // Buyer name must be provided.
            if (string.IsNullOrWhiteSpace(opportunity.BuyerName))
            {
                return false;
            }

            // Purchase order number must be provided.
            if (string.IsNullOrWhiteSpace(opportunity.PONumber))
            {
                return false;
            }

            // The PO must have a positive commercial value.
            if (opportunity.POValue <= 0)
            {
                return false;
            }

            // Issue date must exist.
            if (opportunity.IssueDate == default)
            {
                return false;
            }

            // Delivery date must exist.
            if (opportunity.DeliveryDate == default)
            {
                return false;
            }

            // If all required information is present,
            // the internal structural check passes.
            return true;
        }
    }
}