using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VERA.Data.Context;
using VERA.Models.Entities;
using VERA.Registry.Data;
using VERA.Registry.Services;
using VERA.Web.Models;

namespace VERA.Web.Controllers
{
    // Handles SME pages
    public class SMEController : Controller
    {
        private readonly VeraDbContext _context;
        private readonly RegistryDbContext _registryDbContext;
        private readonly PdfDocumentAnalysisService _pdfDocumentAnalysisService;


        // Get the services used by the SME flow
        public SMEController(
            VeraDbContext context,
            RegistryDbContext registryDbContext,
            PdfDocumentAnalysisService pdfDocumentAnalysisService)
        {
            _context = context;
            _registryDbContext = registryDbContext;
            _pdfDocumentAnalysisService = pdfDocumentAnalysisService;
        }


        // Opens the SME dashboard
        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }


        // Opens the SME opportunities page
        [HttpGet]
        public IActionResult Opportunities()
        {
            return View();
        }


        // Opens the new opportunity form
        [HttpGet]
        public IActionResult NewOpportunity()
        {
            return View(new NewOpportunityViewModel());
        }


        // Creates and verifies a new opportunity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewOpportunity(
            NewOpportunityViewModel model)
        {
            // Return the form if required information is missing
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // Make sure a purchase order was uploaded
            if (model.PurchaseOrderFile == null ||
                model.PurchaseOrderFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.PurchaseOrderFile),
                    "Please upload a purchase order.");

                return View(model);
            }


            // Only accept PDF purchase orders for this flow
            if (!string.Equals(
                Path.GetExtension(model.PurchaseOrderFile.FileName),
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(model.PurchaseOrderFile),
                    "Please upload the purchase order as a PDF.");

                return View(model);
            }


            // Limit the PO file size to 10 MB
            const long maxFileSize =
                10 * 1024 * 1024;

            if (model.PurchaseOrderFile.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(model.PurchaseOrderFile),
                    "The purchase order PDF must be smaller than 10 MB.");

                return View(model);
            }


            // Create a temporary path for the uploaded PDF
            string tempFilePath =
                Path.Combine(
                    Path.GetTempPath(),
                    $"{Guid.NewGuid()}.pdf");


            try
            {
                // Save the PDF temporarily
                await using (var stream = new FileStream(
                    tempFilePath,
                    FileMode.Create))
                {
                    await model.PurchaseOrderFile
                        .CopyToAsync(stream);
                }


                // Read the purchase order
                var pdfResult =
                    _pdfDocumentAnalysisService
                        .Analyse(tempFilePath);


                // Stop if the PDF could not be read
                if (!pdfResult.TextExtracted)
                {
                    ModelState.AddModelError(
                        nameof(model.PurchaseOrderFile),
                        pdfResult.ErrorMessage ??
                        "VERA could not read this purchase order.");

                    return View(model);
                }


                // Stop if the document does not look like a PO
                if (!pdfResult.LooksLikePurchaseOrder)
                {
                    ModelState.AddModelError(
                        nameof(model.PurchaseOrderFile),
                        "The uploaded document does not appear to be a purchase order.");

                    return View(model);
                }


                // Make sure a PO number was found
                if (string.IsNullOrWhiteSpace(
                    pdfResult.ExtractedPONumber))
                {
                    ModelState.AddModelError(
                        nameof(model.PurchaseOrderFile),
                        "VERA could not find a PO number in the document.");

                    return View(model);
                }


                // Make sure a PO amount was found
                if (!pdfResult.ExtractedAmount.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(model.PurchaseOrderFile),
                        "VERA could not find the PO value in the document.");

                    return View(model);
                }


                // Check that the entered PO number matches the PDF
                if (!string.Equals(
                    pdfResult.ExtractedPONumber.Trim(),
                    model.PurchaseOrderNumber.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        nameof(model.PurchaseOrderNumber),
                        "The PO number does not match the uploaded purchase order.");

                    return View(model);
                }


                // Check that the entered PO value matches the PDF
                if (pdfResult.ExtractedAmount.Value !=
                    model.PurchaseOrderValue)
                {
                    ModelState.AddModelError(
                        nameof(model.PurchaseOrderValue),
                        "The PO value does not match the uploaded purchase order.");

                    return View(model);
                }


                // Use the demo SME until user accounts are linked to businesses
                const int businessId = 1;


                // Find the SME business profile
                VERA.Models.Entities.Business? business =
                    await _context.Businesses
                        .FirstOrDefaultAsync(b =>
                            b.BusinessId == businessId);


                // Stop if the business profile does not exist
                if (business == null)
                {
                    return BadRequest(
                        "The SME business profile could not be found.");
                }


                // Create the new opportunity
                Opportunity opportunity = new()
                {
                    BusinessId =
                        business.BusinessId,

                    BuyerName =
                        model.BuyerName,

                    BuyerReference =
                        model.BuyerReference ?? string.Empty,

                    PONumber =
                        model.PurchaseOrderNumber,

                    POValue =
                        model.PurchaseOrderValue,

                    FulfilmentCost =
                        model.FulfilmentCost,

                    SMEContribution =
                        model.SMEContribution,

                    IssueDate =
                        model.IssueDate,

                    DeliveryDate =
                        model.DeliveryDate,

                    UploadedPOFileName =
                        model.PurchaseOrderFile.FileName,

                    CreatedAt =
                        DateTime.UtcNow
                };


                // Save the opportunity
                _context.Opportunities.Add(opportunity);

                await _context.SaveChangesAsync();


                // Look for the matching PO in the Registry
                var registeredPO =
                    await _registryDbContext
                        .RegisteredPurchaseOrders
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p =>
                            p.PONumber == model.PurchaseOrderNumber);


                // Send Registry details into the assessment when a match exists
                if (registeredPO != null)
                {
                    return RedirectToAction(
                        "Assess",
                        "Assessment",
                        new
                        {
                            id =
                                opportunity.OpportunityId,

                            veraPOId =
                                registeredPO.VeraPOId,

                            supplierName =
                                business.BusinessName
                        });
                }


                // Run the assessment without Registry details if no record exists
                return RedirectToAction(
                    "Assess",
                    "Assessment",
                    new
                    {
                        id =
                            opportunity.OpportunityId
                    });
            }
            finally
            {
                // Delete the temporary PDF
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }
    }
}