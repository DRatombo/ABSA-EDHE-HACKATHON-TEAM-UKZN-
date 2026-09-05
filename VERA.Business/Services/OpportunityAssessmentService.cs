using VERA.Models.Entities;
using VERA.Models.Enums;
using VERA.Models.Enums.VERA.Models.Enums;
using VERA.Registry.Models.ViewModels;

namespace VERA.Business.Services
{
    // Runs all the checks needed to assess an opportunity
    public class OpportunityAssessmentService
    {
        private readonly FundingCalculatorService _fundingCalculatorService;
        private readonly FingerprintService _fingerprintService;
        private readonly DuplicateDetectionService _duplicateDetectionService;
        private readonly VerificationService _verificationService;
        private readonly FulfilmentAssessmentService _fulfilmentAssessmentService;
        private readonly OpportunityDecisionService _opportunityDecisionService;


        // Get the services used during the assessment
        public OpportunityAssessmentService(
            FundingCalculatorService fundingCalculatorService,
            FingerprintService fingerprintService,
            DuplicateDetectionService duplicateDetectionService,
            VerificationService verificationService,
            FulfilmentAssessmentService fulfilmentAssessmentService,
            OpportunityDecisionService opportunityDecisionService)
        {
            _fundingCalculatorService = fundingCalculatorService;
            _fingerprintService = fingerprintService;
            _duplicateDetectionService = duplicateDetectionService;
            _verificationService = verificationService;
            _fulfilmentAssessmentService = fulfilmentAssessmentService;
            _opportunityDecisionService = opportunityDecisionService;
        }


        // Assess one opportunity using the opportunities already stored
        public Opportunity Assess(
            Opportunity opportunity,
            IEnumerable<Opportunity> existingOpportunities,
            VerifyPOResult? registryResult = null)
        {
            // Convert to a list because it is used more than once
            List<Opportunity> existingOpportunityList =
                existingOpportunities.ToList();


            // Calculate the funding gap and SME margin
            _fundingCalculatorService.Calculate(opportunity);


            // Create a fingerprint for duplicate checking
            opportunity.Fingerprint =
                _fingerprintService.Generate(opportunity);


            // Check whether a similar opportunity already exists
            bool isDuplicate =
                _duplicateDetectionService.IsDuplicate(
                    opportunity,
                    existingOpportunityList);


            // Run the verification checks
            // Include the Registry result when one is available
            List<VerificationResult> verificationResults =
                _verificationService.Verify(
                    opportunity,
                    isDuplicate,
                    registryResult);


            // Save the verification results
            opportunity.VerificationResults =
                verificationResults;


            // Check the risks around fulfilling the opportunity
            List<RiskFlag> riskFlags =
                _fulfilmentAssessmentService.Assess(
                    opportunity,
                    existingOpportunityList);


            // Add a risk flag if the opportunity looks duplicated
            if (isDuplicate)
            {
                riskFlags.Add(new RiskFlag
                {
                    OpportunityId = opportunity.OpportunityId,

                    Category = "Duplicate Financing",

                    Severity = RiskSeverity.High,

                    Description =
                        "A matching opportunity fingerprint already exists " +
                        "in VERA's internal opportunity registry."
                });
            }


            // Add a Registry risk when the Registry blocks the PO
            if (registryResult != null &&
                registryResult.Result == "BLOCK")
            {
                riskFlags.Add(new RiskFlag
                {
                    OpportunityId = opportunity.OpportunityId,

                    Category = "Registry Verification",

                    Severity = RiskSeverity.High,

                    Description =
                        registryResult.ReasonCodes.Any()
                            ? "Registry blocked the opportunity: " +
                              string.Join(", ", registryResult.ReasonCodes)
                            : "Registry blocked the opportunity."
                });
            }


            // Add a review flag when the Registry needs manual review
            if (registryResult != null &&
                registryResult.Result == "REVIEW")
            {
                riskFlags.Add(new RiskFlag
                {
                    OpportunityId = opportunity.OpportunityId,

                    Category = "Registry Verification",

                    Severity = RiskSeverity.Medium,

                    Description =
                        registryResult.ReasonCodes.Any()
                            ? "Registry requires review: " +
                              string.Join(", ", registryResult.ReasonCodes)
                            : "Registry requires manual review."
                });
            }


            // Save the risk flags
            opportunity.RiskFlags =
                riskFlags;


            // Decide the final readiness status
            opportunity.Status =
                _opportunityDecisionService.Decide(
                    opportunity,
                    verificationResults,
                    riskFlags);


            // Return the completed assessment
            return opportunity;
        }
    }
}