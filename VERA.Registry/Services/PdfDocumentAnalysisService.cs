using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using VERA.Registry.Models.ViewModels;

namespace VERA.Registry.Services
{
    // Reads uploaded PDF files and looks for purchase order information
    public class PdfDocumentAnalysisService
    {
        // Analyse a PDF file and return any PO information found
        public PdfAnalysisResult Analyse(string filePath)
        {
            // Create an empty result
            var result = new PdfAnalysisResult();

            try
            {
                // Open the PDF
                using var document = PdfDocument.Open(filePath);

                // Read text from every page
                string fullText = string.Join(
                    Environment.NewLine,
                    document.GetPages()
                        .Select(page =>
                            ContentOrderTextExtractor.GetText(page)));

                // Stop if no readable text was found
                if (string.IsNullOrWhiteSpace(fullText))
                {
                    result.TextExtracted = false;
                    result.LooksLikePurchaseOrder = false;

                    result.ErrorMessage =
                        "No machine-readable text could be extracted from the PDF.";

                    return result;
                }

                // Text was successfully read
                result.TextExtracted = true;


                // Check whether the PDF actually looks like a purchase order
                result.LooksLikePurchaseOrder =
                    LooksLikePurchaseOrder(fullText);


                // Stop if this does not look like a PO
                if (!result.LooksLikePurchaseOrder)
                {
                    result.ErrorMessage =
                        "This document does not appear to be a purchase order.";

                    return result;
                }


                // Try to extract the PO number
                result.ExtractedPONumber =
                    ExtractPONumber(fullText);


                // Try to extract the PO amount
                result.ExtractedAmount =
                    ExtractAmount(fullText);


                // A PO must contain both a PO number and an amount
                if (string.IsNullOrWhiteSpace(result.ExtractedPONumber) ||
                    !result.ExtractedAmount.HasValue)
                {
                    result.LooksLikePurchaseOrder = false;

                    result.ErrorMessage =
                        "The document looks like a purchase order, but VERA could not find both the PO number and total amount.";

                    return result;
                }


                // Valid purchase order document
                return result;
            }
            catch (Exception ex)
            {
                // Stop if the PDF could not be opened or read
                result.TextExtracted = false;
                result.LooksLikePurchaseOrder = false;

                result.ErrorMessage =
                    $"PDF analysis failed: {ex.Message}";

                return result;
            }
        }


        // Check whether the document contains signs of being a purchase order
        private bool LooksLikePurchaseOrder(string text)
        {
            // Convert to lower case so checks are not case-sensitive
            string lowerText = text.ToLowerInvariant();


            // Common purchase order wording
            bool hasPurchaseOrderWords =
                lowerText.Contains("purchase order") ||
                lowerText.Contains("po number") ||
                lowerText.Contains("po no") ||
                lowerText.Contains("po #");


            // Common supplier wording
            bool hasSupplierWords =
                lowerText.Contains("supplier") ||
                lowerText.Contains("vendor");


            // Common amount wording
            bool hasAmountWords =
                lowerText.Contains("grand total") ||
                lowerText.Contains("total amount") ||
                lowerText.Contains("order value") ||
                lowerText.Contains("po value");


            // Require more than just one random matching word
            int signals = 0;

            if (hasPurchaseOrderWords)
            {
                signals++;
            }

            if (hasSupplierWords)
            {
                signals++;
            }

            if (hasAmountWords)
            {
                signals++;
            }


            // Require at least two purchase-order signals
            return signals >= 2;
        }


        // Try to find a PO number in the text
        private string? ExtractPONumber(string text)
        {
            string[] patterns =
            {
                @"(?im)\bPO\s*Number\s*[:\-]?\s*([A-Z0-9][A-Z0-9/_\-.]+)",

                @"(?im)\bPO\s*No\.?\s*[:\-]?\s*([A-Z0-9][A-Z0-9/_\-.]+)",

                @"(?im)\bPO\s*#\s*[:\-]?\s*([A-Z0-9][A-Z0-9/_\-.]+)",

                @"(?im)\bPurchase\s*Order\s*(?:Number|No\.?)?\s*[:\-]?\s*([A-Z0-9][A-Z0-9/_\-.]+)",

                @"(?im)\bOrder\s*No\.?\s*[:\-]?\s*([A-Z0-9][A-Z0-9/_\-.]+)"
            };


            // Try each PO number pattern
            foreach (string pattern in patterns)
            {
                Match match = Regex.Match(
                    text,
                    pattern,
                    RegexOptions.IgnoreCase);


                // Return the first PO number found
                if (match.Success)
                {
                    return match.Groups[1]
                        .Value
                        .Trim()
                        .TrimEnd('.', ',', ';');
                }
            }


            // No PO number found
            return null;
        }


        // Try to find the total PO amount
        private decimal? ExtractAmount(string text)
        {
            string[] patterns =
            {
                @"(?im)\bGrand\s*Total\s*[:\-]?\s*(?:R|ZAR)?\s*([0-9][0-9,\s]*(?:\.[0-9]{1,2})?)",

                @"(?im)\bTotal\s*Amount\s*[:\-]?\s*(?:R|ZAR)?\s*([0-9][0-9,\s]*(?:\.[0-9]{1,2})?)",

                @"(?im)\bAmount\s*Due\s*[:\-]?\s*(?:R|ZAR)?\s*([0-9][0-9,\s]*(?:\.[0-9]{1,2})?)",

                @"(?im)\bOrder\s*Value\s*[:\-]?\s*(?:R|ZAR)?\s*([0-9][0-9,\s]*(?:\.[0-9]{1,2})?)",

                @"(?im)\bPO\s*Value\s*[:\-]?\s*(?:R|ZAR)?\s*([0-9][0-9,\s]*(?:\.[0-9]{1,2})?)",

                @"(?im)\bTotal\s*[:\-]?\s*(?:R|ZAR)?\s*([0-9][0-9,\s]*(?:\.[0-9]{1,2})?)"
            };


            // Try each amount pattern
            foreach (string pattern in patterns)
            {
                Match match = Regex.Match(
                    text,
                    pattern,
                    RegexOptions.IgnoreCase);


                // Try the next pattern if nothing matched
                if (!match.Success)
                {
                    continue;
                }


                // Clean the amount before converting it
                string amountText =
                    match.Groups[1]
                        .Value
                        .Replace(",", "")
                        .Replace(" ", "")
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Trim();


                // Convert the text into a decimal amount
                if (decimal.TryParse(
                    amountText,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal amount))
                {
                    return amount;
                }
            }


            // No amount found
            return null;
        }
    }
}