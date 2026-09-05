using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
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
        private readonly ILogger<SMEController> _logger;


        // Get the services used by the SME flow
        public SMEController(
            VeraDbContext context,
            RegistryDbContext registryDbContext,
            PdfDocumentAnalysisService pdfDocumentAnalysisService,
            ILogger<SMEController> logger)
        {
            _context = context;
            _registryDbContext = registryDbContext;
            _pdfDocumentAnalysisService = pdfDocumentAnalysisService;
            _logger = logger;
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


            // Make sure financial values are valid
            if (model.PurchaseOrderValue <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.PurchaseOrderValue),
                    "The purchase order value must be greater than zero.");

                return View(model);
            }


            // Fulfilment cost cannot be negative
            if (model.FulfilmentCost < 0)
            {
                ModelState.AddModelError(
                    nameof(model.FulfilmentCost),
                    "Fulfilment cost cannot be negative.");

                return View(model);
            }


            // SME contribution cannot be negative
            if (model.SMEContribution < 0)
            {
                ModelState.AddModelError(
                    nameof(model.SMEContribution),
                    "SME contribution cannot be negative.");

                return View(model);
            }


            // Delivery must happen after the PO was issued
            if (model.DeliveryDate <= model.IssueDate)
            {
                ModelState.AddModelError(
                    nameof(model.DeliveryDate),
                    "The delivery date must be after the issue date.");

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
            const long maxFileSize = 10 * 1024 * 1024;

            if (model.PurchaseOrderFile.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(model.PurchaseOrderFile),
                    "The purchase order PDF must be smaller than 10 MB.");

                return View(model);
            }


            // A valid PDF needs enough bytes for its file signature
            if (model.PurchaseOrderFile.Length < 5)
            {
                ModelState.AddModelError(
                    nameof(model.PurchaseOrderFile),
                    "The uploaded purchase order is not a valid PDF.");

                return View(model);
            }


            // Check the actual file contents, not only the .pdf extension
            if (!await HasValidPdfSignatureAsync(
                    model.PurchaseOrderFile))
            {
                ModelState.AddModelError(
                    nameof(model.PurchaseOrderFile),
                    "The uploaded file is not a valid PDF.");

                return View(model);
            }


            // Create a random temporary file name
            string tempFilePath =
                Path.Combine(
                    Path.GetTempPath(),
                    $"{Guid.NewGuid():N}.pdf");


            try
            {
                // Save the PDF temporarily
                await using (var stream = new FileStream(
                    tempFilePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true))
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


                // Use the demo SME until accounts are linked to businesses
                const int businessId = 1;


                // Find the SME business profile
                VERA.Models.Entities.Business? business =
                    await _context.Businesses
                        .AsNoTracking()
                        .FirstOrDefaultAsync(b =>
                            b.BusinessId == businessId);


                // Stop if the business profile does not exist
                if (business == null)
                {
                    _logger.LogWarning(
                        "Opportunity submission failed because business {BusinessId} was not found.",
                        businessId);

                    return BadRequest(
                        "The SME business profile could not be found.");
                }


                // Store only the filename and remove any supplied path
                string safeFileName =
                    Path.GetFileName(
                        model.PurchaseOrderFile.FileName);


                // Create the new opportunity
                Opportunity opportunity = new()
                {
                    BusinessId =
                        business.BusinessId,

                    BuyerName =
                        model.BuyerName.Trim(),

                    BuyerReference =
                        model.BuyerReference?.Trim() ??
                        string.Empty,

                    PONumber =
                        model.PurchaseOrderNumber.Trim(),

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
                        safeFileName,

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
                            p.PONumber ==
                            model.PurchaseOrderNumber.Trim());


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
            catch (Exception ex)
            {
                // Keep technical error details out of the browser
                _logger.LogError(
                    ex,
                    "An error occurred while submitting a new SME opportunity.");

                ModelState.AddModelError(
                    string.Empty,
                    "VERA could not process the opportunity. Please try again.");

                return View(model);
            }
            finally
            {
                // Delete the temporary PDF
                try
                {
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                }
                catch (IOException ex)
                {
                    // Log cleanup problems without exposing them to the user
                    _logger.LogWarning(
                        ex,
                        "VERA could not delete temporary file {TempFilePath}.",
                        tempFilePath);
                }
            }
        }


        // Check whether the uploaded file starts with the PDF signature
        private static async Task<bool> HasValidPdfSignatureAsync(
            IFormFile file)
        {
            // PDF files normally start with %PDF-
            byte[] expectedHeader =
                Encoding.ASCII.GetBytes("%PDF-");

            byte[] actualHeader =
                new byte[expectedHeader.Length];

            await using Stream stream =
                file.OpenReadStream();

            int bytesRead =
                await stream.ReadAsync(
                    actualHeader.AsMemory(
                        0,
                        actualHeader.Length));

            if (bytesRead != expectedHeader.Length)
            {
                return false;
            }

            return actualHeader.SequenceEqual(
                expectedHeader);
        }
    }
}