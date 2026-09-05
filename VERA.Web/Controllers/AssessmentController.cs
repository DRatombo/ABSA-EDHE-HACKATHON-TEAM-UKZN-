using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VERA.Business.Services;
using VERA.Data.Context;
using VERA.Models.Entities;

namespace VERA.Web.Controllers
{
    /// <summary>
    /// Provides access to VERA's opportunity assessment engine.
    ///
    /// This controller connects the Web layer to the Business layer.
    /// It allows an opportunity stored in the database to be passed
    /// through VERA's complete assessment pipeline.
    ///
    /// The assessment includes:
    /// - funding calculations
    /// - fingerprint generation
    /// - duplicate detection
    /// - verification profile
    /// - fulfilment risk assessment
    /// - finance-readiness decision
    ///
    /// IMPORTANT:
    /// VERA determines finance-readiness for funder review.
    /// It does not make the final lending decision.
    /// </summary>
    public class AssessmentController : Controller
    {
        private readonly VeraDbContext _context;
        private readonly OpportunityAssessmentService _assessmentService;

        /// <summary>
        /// ASP.NET Core automatically injects the database context
        /// and the VERA assessment service because both were
        /// registered in Program.cs.
        /// </summary>
        public AssessmentController(
            VeraDbContext context,
            OpportunityAssessmentService assessmentService)
        {
            _context = context;
            _assessmentService = assessmentService;
        }

        /// <summary>
        /// Runs the full VERA assessment pipeline for an opportunity
        /// already stored in the database.
        ///
        /// Example URL:
        /// /Assessment/Assess/5
        ///
        /// where 5 is the OpportunityId.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Assess(int id)
        {
            // ---------------------------------------------------------
            // STEP 1: FIND THE CURRENT OPPORTUNITY
            // ---------------------------------------------------------
            //
            // Load the opportunity that the user wants VERA to assess.
            Opportunity? opportunity =
                await _context.Opportunities
                    .FirstOrDefaultAsync(o =>
                        o.OpportunityId == id);

            // If the opportunity does not exist, return HTTP 404.
            if (opportunity == null)
            {
                return NotFound(
                    $"No opportunity with ID {id} was found.");
            }


            // ---------------------------------------------------------
            // STEP 2: LOAD EXISTING OPPORTUNITIES
            // ---------------------------------------------------------
            //
            // VERA needs previous opportunities for:
            //
            // - duplicate detection
            // - previous fulfilment history
            // - scale-risk assessment
            //
            // We exclude the opportunity currently being assessed.
            List<Opportunity> existingOpportunities =
                await _context.Opportunities
                    .AsNoTracking()
                    .Where(o =>
                        o.OpportunityId != id)
                    .ToListAsync();


            // ---------------------------------------------------------
            // STEP 3: RUN THE VERA ASSESSMENT ENGINE
            // ---------------------------------------------------------
            //
            // This one call executes the complete pipeline:
            //
            // Funding calculations
            //      ↓
            // Fingerprint
            //      ↓
            // Duplicate detection
            //      ↓
            // Verification profile
            //      ↓
            // Fulfilment assessment
            //      ↓
            // Final readiness decision
            Opportunity assessedOpportunity =
                _assessmentService.Assess(
                    opportunity,
                    existingOpportunities);


            // ---------------------------------------------------------
            // STEP 4: RETURN THE ASSESSMENT
            // ---------------------------------------------------------
            //
            // For now, return JSON so that we can prove that the
            // controller and business layer work together correctly.
            //
            // The Web/UI teammate can later replace this with a
            // Razor View using the same service.
            return Json(new
            {
                assessedOpportunity.OpportunityId,

                assessedOpportunity.BusinessId,

                assessedOpportunity.BuyerName,

                assessedOpportunity.PONumber,

                assessedOpportunity.POValue,

                assessedOpportunity.FulfilmentCost,

                assessedOpportunity.SMEContribution,

                assessedOpportunity.FundingGap,

                assessedOpportunity.EstimatedFundingCost,

                assessedOpportunity.PlatformFee,

                assessedOpportunity.RemainingMargin,

                assessedOpportunity.RemainingMarginPercentage,

                assessedOpportunity.Fingerprint,

                Status =
                    assessedOpportunity.Status.ToString(),

                // Return the explainable verification profile.
                VerificationResults =
                    assessedOpportunity.VerificationResults
                        .Select(v => new
                        {
                            v.VerificationType,

                            Status =
                                v.Status.ToString(),

                            v.Evidence,

                            v.Source,

                            v.IsSimulated,

                            v.CheckedAt
                        }),

                // Return the explainable fulfilment/risk indicators.
                RiskFlags =
                    assessedOpportunity.RiskFlags
                        .Select(r => new
                        {
                            r.Category,

                            Severity =
                                r.Severity.ToString(),

                            r.Description
                        })
            });
        }
    }
}