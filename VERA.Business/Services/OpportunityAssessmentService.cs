using VERA.Models.Entities;
using VERA.Models.Enums;
using VERA.Models.Enums.VERA.Models.Enums;

namespace VERA.Business.Services
{
    /// <summary>
    /// Coordinates the full VERA opportunity assessment workflow.
    ///
    /// This service brings together:
    /// - funding calculations
    /// - PO fingerprint generation
    /// - duplicate detection
    /// - verification checks
    /// - fulfilment assessment
    /// - final opportunity decision
    ///
    /// The Web layer can call this single service instead of calling
    /// each business service individually.
    /// </summary>
    public class OpportunityAssessmentService
    {
        private readonly FundingCalculatorService _fundingCalculatorService;
        private readonly FingerprintService _fingerprintService;
        private readonly DuplicateDetectionService _duplicateDetectionService;
        private readonly VerificationService _verificationService;
        private readonly FulfilmentAssessmentService _fulfilmentAssessmentService;
        private readonly OpportunityDecisionService _opportunityDecisionService;

        /// <summary>
        /// Constructor used to inject all services required
        /// for a complete VERA assessment.
        /// </summary>
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

        /// <summary>
        /// Runs the full assessment process for a single opportunity.
        /// </summary>
        /// <param name="opportunity">
        /// The new opportunity currently being assessed.
        /// </param>
        /// <param name="existingOpportunities">
        /// Opportunities already stored in VERA.
        /// These are used for duplicate detection and fulfilment history.
        /// </param>
        /// <returns>
        /// The same opportunity after calculations, verification,
        /// risk assessment and final status assignment have been applied.
        /// </returns>
        public Opportunity Assess(
            Opportunity opportunity,
            IEnumerable<Opportunity> existingOpportunities)
        {
            // Convert the existing opportunities into a list because
            // multiple services need to inspect the same data.
            List<Opportunity> existingOpportunityList =
                existingOpportunities.ToList();

            // ---------------------------------------------------------
            // STEP 1: CALCULATE FUNDING ECONOMICS
            // ---------------------------------------------------------
            //
            // Determine:
            // - how much external capital is required
            // - remaining SME margin
            // - remaining SME margin percentage
            _fundingCalculatorService.Calculate(opportunity);

            // ---------------------------------------------------------
            // STEP 2: GENERATE THE OPPORTUNITY FINGERPRINT
            // ---------------------------------------------------------
            //
            // The fingerprint is used to identify potentially duplicated
            // funding requests within VERA.
            opportunity.Fingerprint =
                _fingerprintService.Generate(opportunity);

            // ---------------------------------------------------------
            // STEP 3: CHECK FOR DUPLICATE SUBMISSIONS
            // ---------------------------------------------------------
            //
            // This only checks VERA's internal opportunity registry.
            // It does not claim cross-bank or cross-funder coverage.
            bool isDuplicate =
                _duplicateDetectionService.IsDuplicate(
                    opportunity,
                    existingOpportunityList);

            // ---------------------------------------------------------
            // STEP 4: RUN VERIFICATION PROFILE
            // ---------------------------------------------------------
            //
            // Creates explainable verification results for:
            // - identity
            // - business legitimacy
            // - buyer
            // - PO structure
            // - duplicate financing
            // - supplier readiness
            List<VerificationResult> verificationResults =
                _verificationService.Verify(
                    opportunity,
                    isDuplicate);

            // Store the verification profile directly on the opportunity.
            opportunity.VerificationResults =
                verificationResults;

            // ---------------------------------------------------------
            // STEP 5: RUN FULFILMENT ASSESSMENT
            // ---------------------------------------------------------
            //
            // Assess execution-related risks such as:
            // - limited history
            // - opportunity size relative to previous work
            // - delivery timeline
            // - dependence on external capital
            List<RiskFlag> riskFlags =
                _fulfilmentAssessmentService.Assess(
                    opportunity,
                    existingOpportunityList);

            // If a duplicate was detected, create an explicit risk flag
            // so that the issue is visible alongside the other risks.
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

            // Store the explainable risk flags on the opportunity.
            opportunity.RiskFlags =
                riskFlags;

            // ---------------------------------------------------------
            // STEP 6: DETERMINE FINAL READINESS STATUS
            // ---------------------------------------------------------
            //
            // The decision engine evaluates all available evidence
            // and determines whether the opportunity is:
            //
            // - FinanceReady
            // - ManualReview
            // - VerificationComplete
            // - Blocked
            //
            // This is NOT a lending approval decision.
            opportunity.Status =
                _opportunityDecisionService.Decide(
                    opportunity,
                    verificationResults,
                    riskFlags);

            // ---------------------------------------------------------
            // STEP 7: RETURN THE ASSESSED OPPORTUNITY
            // ---------------------------------------------------------
            //
            // The caller now receives one opportunity containing:
            // - calculated financial values
            // - fingerprint
            // - verification results
            // - risk flags
            // - final readiness status
            return opportunity;
        }
    }
}