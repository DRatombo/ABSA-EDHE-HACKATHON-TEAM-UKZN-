using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VERA.Registry.Data;
using VERA.Registry.Models.ViewModels;
using VERA.Web.Models;

namespace VERA.Web.Controllers
{
    // Handles Registry pages and Registry verification
    public class RegistryController : Controller
    {
        // Gives access to the Registry database
        private readonly RegistryDbContext _registryDbContext;

        // Runs the Registry verification checks
        private readonly VERA.Registry.Services.RegistryVerificationService _registryVerificationService;

        // Reads PO information from uploaded PDF files
        private readonly VERA.Registry.Services.PdfDocumentAnalysisService _pdfDocumentAnalysisService;


        // ASP.NET gives the controller these services automatically
        public RegistryController(
            RegistryDbContext registryDbContext,
            VERA.Registry.Services.RegistryVerificationService registryVerificationService,
            VERA.Registry.Services.PdfDocumentAnalysisService pdfDocumentAnalysisService)
        {
            _registryDbContext = registryDbContext;
            _registryVerificationService = registryVerificationService;
            _pdfDocumentAnalysisService = pdfDocumentAnalysisService;
        }


        // Opens the main Registry page
        [HttpGet]
        public IActionResult Index()
        {
            // Start with an empty form
            return View(new RegistryVerifyViewModel());
        }


        // Reads PO details from an uploaded PDF
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnalyseDocument(
            RegistryVerifyViewModel model)
        {
            // Make sure a file was selected
            if (model.Document == null ||
                model.Document.Length == 0)
            {
                model.PdfAnalysed = false;

                model.PdfMessage =
                    "Please choose a PDF file first.";

                return View("Index", model);
            }


            // Only allow PDF files
            if (!string.Equals(
                Path.GetExtension(model.Document.FileName),
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                model.PdfAnalysed = false;

                model.PdfMessage =
                    "Only PDF purchase orders can be analysed.";

                return View("Index", model);
            }


            // Limit uploaded files to 10 MB
            // This stops very large files from being uploaded
            const long maxFileSize =
                10 * 1024 * 1024;

            if (model.Document.Length > maxFileSize)
            {
                model.PdfAnalysed = false;

                model.PdfMessage =
                    "The PDF must be smaller than 10 MB.";

                return View("Index", model);
            }


            // Create a temporary file path
            // The PDF service needs a file path to read the document
            var tempFilePath =
                Path.Combine(
                    Path.GetTempPath(),
                    $"{Guid.NewGuid()}.pdf");


            try
            {
                // Save the uploaded PDF temporarily
                await using (var stream = new FileStream(
                    tempFilePath,
                    FileMode.Create))
                {
                    await model.Document.CopyToAsync(stream);
                }


                // Analyse the PDF
                var result =
                    _pdfDocumentAnalysisService
                        .Analyse(tempFilePath);


                // Stop if text could not be read
                if (!result.TextExtracted)
                {
                    model.PdfAnalysed = false;

                    model.PdfMessage =
                        result.ErrorMessage ??
                        "The PDF could not be analysed.";

                    return View("Index", model);
                }


                // Stop if this does not look like a purchase order
                if (!result.LooksLikePurchaseOrder)
                {
                    model.PdfAnalysed = false;

                    model.PdfMessage =
                        result.ErrorMessage ??
                        "This document does not appear to be a purchase order.";

                    return View("Index", model);
                }


                // Stop if a PO number was not found
                if (string.IsNullOrWhiteSpace(
                    result.ExtractedPONumber))
                {
                    model.PdfAnalysed = false;

                    model.PdfMessage =
                        "VERA could not find a purchase order number in this document.";

                    return View("Index", model);
                }


                // Stop if a PO amount was not found
                if (!result.ExtractedAmount.HasValue)
                {
                    model.PdfAnalysed = false;

                    model.PdfMessage =
                        "VERA could not find a purchase order amount in this document.";

                    return View("Index", model);
                }


                // Fill the PO number using the value found in the PDF
                model.PONumber =
                    result.ExtractedPONumber;


                // Fill the PO amount using the value found in the PDF
                model.Amount =
                    result.ExtractedAmount.Value;


                // ASP.NET remembers the old form values that were posted
                // Clear them so the page uses the new extracted values
                ModelState.Clear();


                // Tell the page that the PDF was accepted
                model.PdfAnalysed = true;


                // Show a success message
                model.PdfMessage =
                    "Purchase order recognised. VERA extracted the PO number and amount. Please confirm the remaining details before verification.";


                // Return to the Registry page
                // The extracted values should now appear in the fields
                return View("Index", model);
            }
            catch (Exception)
            {
                // Show a simple message if something unexpected goes wrong
                model.PdfAnalysed = false;

                model.PdfMessage =
                    "Something went wrong while reading the PDF. Please try again.";

                return View("Index", model);
            }
            finally
            {
                // Delete the temporary PDF after analysis
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }


        // Runs the actual Registry verification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(
            RegistryVerifyViewModel model)
        {
            // The PDF is not uploaded again during final verification
            ModelState.Remove(nameof(model.Document));


            // Return to the form if required details are missing
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }


            // Convert the Web form into the Registry request model
            var request = new VerifyPORequest
            {
                VeraPOId = model.VeraPOId,
                PONumber = model.PONumber,
                SupplierName = model.SupplierName,
                Amount = model.Amount
            };


            // Run the Registry checks
            var result =
                await _registryVerificationService
                    .VerifyAsync(request);


            // Show the Registry result page
            return View("Result", result);
        }


        // Shows registered POs for internal testing
        [HttpGet]
        public async Task<IActionResult> RegisteredPurchaseOrders()
        {
            // Get Registry records with issuer and financing information
            var purchaseOrders =
                await _registryDbContext
                    .RegisteredPurchaseOrders
                    .Include(p => p.RegistryIssuer)
                    .Include(p => p.FinancingClaims)
                    .OrderByDescending(p => p.RegisteredAt)
                    .ToListAsync();


            // Send the records to the testing page
            return View(purchaseOrders);
        }
    }
}