using VERA.Models.Entities;
using VERA.Models.Enums;
using VERA.Models.Enums.VERA.Models.Enums;
using VERA.Registry.Models.ViewModels;

namespace VERA.Business.Services
{
    // Builds the verification profile for an opportunity
    public class VerificationService
    {
        // Run the verification checks
        public List<VerificationResult> Verify(
            Opportunity opportunity,
            bool isDuplicate,
            VerifyPOResult? registryResult = null)
        {
            // Store all verification results here
            List<VerificationResult> results = new();


            // Identity check
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


            // Business registration check
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


            // Buyer check
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


            // Check that the PO has the required fields
            VerificationStatus poStatus =
                IsPODataComplete(opportunity)
                    ? VerificationStatus.Verified
                    : VerificationStatus.Flagged;


            // Add the PO check result
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


            // Add the Registry result when a Registry check was run
            if (registryResult != null)
            {
                // Convert the Registry result to a verification status
                VerificationStatus registryStatus =
                    registryResult.Result == "PASS"
                        ? VerificationStatus.Verified
                        : VerificationStatus.Flagged;


                // Build a simple message for the verification profile
                string registryEvidence;

                if (registryResult.Result == "PASS")
                {
                    registryEvidence =
                        "The purchase order matched the Registry record and no blocking issue was found.";
                }
                else if (registryResult.ReasonCodes.Any())
                {
                    registryEvidence =
                        $"Registry result: {registryResult.Result}. " +
                        string.Join(", ", registryResult.ReasonCodes);
                }
                else
                {
                    registryEvidence =
                        $"Registry result: {registryResult.Result}.";
                }


                // Add the Registry check
                results.Add(new VerificationResult
                {
                    OpportunityId = opportunity.OpportunityId,

                    VerificationType = "VERA Registry Check",

                    Status = registryStatus,

                    Evidence = registryEvidence,

                    Source = "VERA Registry",

                    IsSimulated = false,

                    CheckedAt = DateTime.UtcNow
                });
            }


            // Check for a duplicate opportunity inside VERA
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


            // Supplier readiness check
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


            // Return all checks
            return results;
        }


        // Check that the PO has the minimum required information
        private bool IsPODataComplete(Opportunity opportunity)
        {
            // Buyer is required
            if (string.IsNullOrWhiteSpace(opportunity.BuyerName))
            {
                return false;
            }


            // PO number is required
            if (string.IsNullOrWhiteSpace(opportunity.PONumber))
            {
                return false;
            }


            // PO value must be greater than zero
            if (opportunity.POValue <= 0)
            {
                return false;
            }


            // Issue date is required
            if (opportunity.IssueDate == default)
            {
                return false;
            }


            // Delivery date is required
            if (opportunity.DeliveryDate == default)
            {
                return false;
            }


            // All required fields are present
            return true;
        }
    }
}