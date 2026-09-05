using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VERA.Business.Services;
using VERA.Data.Context;
using VERA.Models.Entities;
using VERA.Registry.Models.ViewModels;
using VERA.Registry.Services;

namespace VERA.Web.Controllers
{
    // Handles opportunity assessments
    public class AssessmentController : Controller
    {
        private readonly VeraDbContext _context;
        private readonly OpportunityAssessmentService _assessmentService;
        private readonly RegistryVerificationService _registryVerificationService;


        // Get the database and services used for assessments
        public AssessmentController(
            VeraDbContext context,
            OpportunityAssessmentService assessmentService,
            RegistryVerificationService registryVerificationService)
        {
            _context = context;
            _assessmentService = assessmentService;
            _registryVerificationService = registryVerificationService;
        }


        // Assess an opportunity that is already stored in VERA
        [HttpGet]
        public async Task<IActionResult> Assess(
            int id,
            string? veraPOId = null,
            string? supplierName = null)
        {
            // Find the opportunity being assessed
            Opportunity? opportunity =
                await _context.Opportunities
                    .FirstOrDefaultAsync(o =>
                        o.OpportunityId == id);


            // Stop if the opportunity does not exist
            if (opportunity == null)
            {
                return NotFound(
                    $"No opportunity with ID {id} was found.");
            }


            // Load the other opportunities used for duplicate and risk checks
            List<Opportunity> existingOpportunities =
                await _context.Opportunities
                    .AsNoTracking()
                    .Where(o =>
                        o.OpportunityId != id)
                    .ToListAsync();


            // Registry result stays empty if no Registry details were supplied
            VerifyPOResult? registryResult = null;


            // Run the Registry check when the required Registry details are available
            if (!string.IsNullOrWhiteSpace(veraPOId) &&
                !string.IsNullOrWhiteSpace(supplierName))
            {
                // Build the Registry request using the stored opportunity
                var registryRequest = new VerifyPORequest
                {
                    VeraPOId = veraPOId,
                    PONumber = opportunity.PONumber,
                    SupplierName = supplierName,
                    Amount = opportunity.POValue
                };


                // Check the PO against the VERA Registry
                registryResult =
                    await _registryVerificationService
                        .VerifyAsync(registryRequest);
            }


            // Run the full opportunity assessment
            Opportunity assessedOpportunity =
                _assessmentService.Assess(
                    opportunity,
                    existingOpportunities,
                    registryResult);


            // Return the assessment for testing
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


                // Show whether the Registry was included
                RegistryCheck =
                    registryResult == null
                        ? null
                        : new
                        {
                            registryResult.RecordFound,

                            registryResult.PONumberMatch,

                            registryResult.SupplierMatch,

                            registryResult.AmountMatch,

                            registryResult.IsActive,

                            registryResult.HasActiveFinancingClaim,

                            registryResult.Result,

                            registryResult.ReasonCodes
                        },


                // Show the verification profile
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


                // Show the risk flags
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