using VERA.Business.Services;
using VERA.Models.Entities;
using VERA.Models.Enums;
using VERA.Models.Enums.VERA.Models.Enums;

namespace VERA.Tests
{
    /// <summary>
    /// Unit tests for VERA's core opportunity assessment logic.
    ///
    /// These tests prove that VERA can:
    /// 1. Calculate the funding gap and SME margin correctly.
    /// 2. Detect risky jumps in opportunity size.
    /// 3. Detect duplicate opportunity submissions.
    /// 4. Block opportunities with critical delivery-window risks.
    ///
    /// These are business-logic tests only and do not require
    /// the database or website to be running.
    /// </summary>
    public class OpportunityAssessmentTests
    {
        
        /// Creates a complete OpportunityAssessmentService
        /// containing all of the services required by the
        /// VERA assessment pipeline.
        private OpportunityAssessmentService CreateAssessmentService()
        {
            // Service responsible for calculating funding economics.
            FundingCalculatorService fundingCalculatorService =
                new FundingCalculatorService();

            // Service responsible for generating a unique
            // fingerprint for an opportunity.
            FingerprintService fingerprintService =
                new FingerprintService();

            // Service responsible for checking whether an
            // opportunity already exists in VERA.
            DuplicateDetectionService duplicateDetectionService =
                new DuplicateDetectionService();

            // Service responsible for producing the
            // explainable verification profile.
            VerificationService verificationService =
                new VerificationService();

            // Service responsible for identifying fulfilment risks.
            FulfilmentAssessmentService fulfilmentAssessmentService =
                new FulfilmentAssessmentService();

            // Service responsible for determining the final
            // finance-readiness status.
            OpportunityDecisionService opportunityDecisionService =
                new OpportunityDecisionService();

            // Combine all of the individual services into
            // VERA's main assessment pipeline.
            return new OpportunityAssessmentService(
                fundingCalculatorService,
                fingerprintService,
                duplicateDetectionService,
                verificationService,
                fulfilmentAssessmentService,
                opportunityDecisionService
            );
        }


        // =========================================================
        // TEST 1: NORMAL OPPORTUNITY
        // =========================================================

        /// <summary>
        /// Tests that a normal opportunity with positive economics
        /// and no serious risk indicators becomes FinanceReady.
        /// </summary>
        [Fact]
        public void NormalOpportunity_ShouldBecomeFinanceReady()
        {
            // -------------------------
            // ARRANGE
            // -------------------------

            // Create the VERA assessment engine.
            OpportunityAssessmentService service =
                CreateAssessmentService();

            // Create a sample opportunity.
            Opportunity opportunity = new Opportunity
            {
                OpportunityId = 1,
                BusinessId = 1,

                BuyerName = "Example Corporate Buyer",
                PONumber = "PO-2026-001",

                // Total value of the purchase order.
                POValue = 480000m,

                // Estimated amount required to fulfil the PO.
                FulfilmentCost = 365000m,

                // Amount the SME can contribute itself.
                SMEContribution = 60000m,

                // Estimated cost charged by the funding provider.
                EstimatedFundingCost = 15000m,

                // Illustrative VERA platform fee.
                PlatformFee = 4500m,

                // Valid delivery window.
                IssueDate = new DateTime(2026, 9, 1),
                DeliveryDate = new DateTime(2026, 9, 30)
            };

            // This scenario has no previous opportunities.
            //
            // VERA should therefore identify limited history,
            // but this is only a Medium risk under our current rules.
            List<Opportunity> existingOpportunities =
                new List<Opportunity>();


            // -------------------------
            // ACT
            // -------------------------

            // Run the opportunity through the complete
            // VERA assessment pipeline.
            Opportunity assessedOpportunity =
                service.Assess(
                    opportunity,
                    existingOpportunities);


            // -------------------------
            // ASSERT
            // -------------------------

            // Funding Gap:
            //
            // R365,000 fulfilment cost
            // - R60,000 SME contribution
            // = R305,000 required funding.
            Assert.Equal(
                305000m,
                assessedOpportunity.FundingGap);

            // Remaining Margin:
            //
            // R480,000 PO value
            // - R365,000 fulfilment cost
            // - R15,000 funding cost
            // - R4,500 platform fee
            // = R95,500 remaining margin.
            Assert.Equal(
                95500m,
                assessedOpportunity.RemainingMargin);

            // Remaining margin percentage:
            //
            // R95,500 / R480,000 × 100
            // ≈ 19.90%
            Assert.Equal(
                19.90m,
                Math.Round(
                    assessedOpportunity.RemainingMarginPercentage,
                    2));

            // VERA should generate a fingerprint for the opportunity.
            Assert.False(
                string.IsNullOrWhiteSpace(
                    assessedOpportunity.Fingerprint));

            // The assessment should produce verification results.
            Assert.NotEmpty(
                assessedOpportunity.VerificationResults);

            // There are no High or Critical risks in this scenario,
            // so the opportunity should be finance-ready for
            // funder review.
            Assert.Equal(
                OpportunityStatus.FinanceReady,
                assessedOpportunity.Status);
        }


        // =========================================================
        // TEST 2: LARGE OPPORTUNITY / SCALE RISK
        // =========================================================

        /// <summary>
        /// Tests whether VERA identifies a major increase in
        /// opportunity size compared with the SME's previous
        /// completed work.
        /// </summary>
        [Fact]
        public void LargeScaleJump_ShouldRequireManualReview()
        {
            // -------------------------
            // ARRANGE
            // -------------------------

            OpportunityAssessmentService service =
                CreateAssessmentService();

            // The SME has now won a R480,000 opportunity.
            Opportunity currentOpportunity =
                new Opportunity
                {
                    OpportunityId = 20,
                    BusinessId = 1,

                    BuyerName = "Example Corporate Buyer",
                    PONumber = "PO-2026-020",

                    POValue = 480000m,
                    FulfilmentCost = 365000m,
                    SMEContribution = 60000m,

                    EstimatedFundingCost = 15000m,
                    PlatformFee = 4500m,

                    IssueDate =
                        new DateTime(2026, 9, 1),

                    DeliveryDate =
                        new DateTime(2026, 9, 30)
                };

            // The SME's largest previous completed PO
            // was only R118,000.
            Opportunity previousOpportunity =
                new Opportunity
                {
                    OpportunityId = 10,
                    BusinessId = 1,

                    BuyerName = "Previous Buyer",
                    PONumber = "PO-2026-010",

                    POValue = 118000m,

                    // Only completed opportunities should form
                    // part of the SME's proven fulfilment history.
                    Status =
                        OpportunityStatus.Completed
                };

            List<Opportunity> existingOpportunities =
                new List<Opportunity>
                {
                    previousOpportunity
                };


            // -------------------------
            // ACT
            // -------------------------

            Opportunity assessedOpportunity =
                service.Assess(
                    currentOpportunity,
                    existingOpportunities);


            // -------------------------
            // ASSERT
            // -------------------------

            // R480,000 is more than twice the previous
            // completed maximum of R118,000.
            //
            // VERA should therefore create a High Scale Risk.
            Assert.Contains(
                assessedOpportunity.RiskFlags,
                risk =>
                    risk.Category == "Scale Risk" &&
                    risk.Severity == RiskSeverity.High);

            // A High risk does not automatically mean fraud
            // or rejection.
            //
            // VERA should send the opportunity for Manual Review.
            Assert.Equal(
                OpportunityStatus.ManualReview,
                assessedOpportunity.Status);
        }


        // =========================================================
        // TEST 3: DUPLICATE OPPORTUNITY
        // =========================================================

        /// <summary>
        /// Tests whether VERA detects a duplicate opportunity
        /// using its deterministic opportunity fingerprint.
        /// </summary>
        [Fact]
        public void DuplicateOpportunity_ShouldBeFlagged()
        {
            // -------------------------
            // ARRANGE
            // -------------------------

            OpportunityAssessmentService service =
                CreateAssessmentService();

            FingerprintService fingerprintService =
                new FingerprintService();

            // Create an opportunity that already exists
            // in VERA's internal opportunity registry.
            Opportunity existingOpportunity =
                new Opportunity
                {
                    OpportunityId = 1,
                    BusinessId = 1,

                    BuyerName = "Example Corporate Buyer",
                    PONumber = "PO-2026-DUP-001",

                    POValue = 100000m
                };

            // In the real application, existing opportunities
            // will already have fingerprints stored.
            //
            // For the test, we generate it manually.
            existingOpportunity.Fingerprint =
                fingerprintService.Generate(
                    existingOpportunity);

            // Create another submission containing the same
            // fingerprint inputs.
            Opportunity duplicateOpportunity =
                new Opportunity
                {
                    OpportunityId = 2,
                    BusinessId = 1,

                    BuyerName = "Example Corporate Buyer",
                    PONumber = "PO-2026-DUP-001",

                    POValue = 100000m,

                    FulfilmentCost = 70000m,
                    SMEContribution = 10000m,

                    EstimatedFundingCost = 4000m,
                    PlatformFee = 1000m,

                    IssueDate =
                        new DateTime(2026, 9, 1),

                    DeliveryDate =
                        new DateTime(2026, 9, 25)
                };

            List<Opportunity> existingOpportunities =
                new List<Opportunity>
                {
                    existingOpportunity
                };


            // -------------------------
            // ACT
            // -------------------------

            Opportunity assessedOpportunity =
                service.Assess(
                    duplicateOpportunity,
                    existingOpportunities);


            // -------------------------
            // ASSERT
            // -------------------------

            // The verification profile should contain
            // a flagged Duplicate Financing Check.
            Assert.Contains(
                assessedOpportunity.VerificationResults,
                verification =>
                    verification.VerificationType ==
                        "Duplicate Financing Check" &&
                    verification.Status ==
                        VerificationStatus.Flagged);

            // The opportunity should also contain an
            // explainable duplicate-financing risk flag.
            Assert.Contains(
                assessedOpportunity.RiskFlags,
                risk =>
                    risk.Category ==
                        "Duplicate Financing");

            // IMPORTANT:
            //
            // A duplicate is a risk signal, not proof of fraud.
            // Therefore VERA sends it for Manual Review
            // instead of automatically rejecting it.
            Assert.Equal(
                OpportunityStatus.ManualReview,
                assessedOpportunity.Status);
        }


        // =========================================================
        // TEST 4: CRITICAL DELIVERY WINDOW
        // =========================================================

        /// <summary>
        /// Tests whether VERA blocks an opportunity when
        /// the delivery date is impossible because it occurs
        /// before the PO issue date.
        /// </summary>
        [Fact]
        public void InvalidDeliveryDate_ShouldBlockOpportunity()
        {
            // -------------------------
            // ARRANGE
            // -------------------------

            OpportunityAssessmentService service =
                CreateAssessmentService();

            Opportunity opportunity =
                new Opportunity
                {
                    OpportunityId = 30,
                    BusinessId = 1,

                    BuyerName = "Example Buyer",
                    PONumber = "PO-2026-BADDATE",

                    POValue = 80000m,
                    FulfilmentCost = 50000m,
                    SMEContribution = 10000m,

                    EstimatedFundingCost = 3000m,
                    PlatformFee = 800m,

                    // The PO is issued on 20 September...
                    IssueDate =
                        new DateTime(2026, 9, 20),

                    // ...but delivery is supposedly due
                    // on 10 September.
                    //
                    // This creates an impossible delivery window.
                    DeliveryDate =
                        new DateTime(2026, 9, 10)
                };


            // -------------------------
            // ACT
            // -------------------------

            Opportunity assessedOpportunity =
                service.Assess(
                    opportunity,
                    new List<Opportunity>());


            // -------------------------
            // ASSERT
            // -------------------------

            // VERA should identify the impossible delivery
            // window as a Critical risk.
            Assert.Contains(
                assessedOpportunity.RiskFlags,
                risk =>
                    risk.Category ==
                        "Delivery Window" &&
                    risk.Severity ==
                        RiskSeverity.Critical);

            // Critical risks should prevent the opportunity
            // from proceeding to funding review.
            Assert.Equal(
                OpportunityStatus.Blocked,
                assessedOpportunity.Status);
        }
    }
}