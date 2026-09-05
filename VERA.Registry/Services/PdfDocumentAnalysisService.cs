using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using VERA.Registry.Models.ViewModels;

namespace VERA.Registry.Services
{
    public class PdfDocumentAnalysisService
    {
        public PdfAnalysisResult Analyse(string filePath)
        {
            var result = new PdfAnalysisResult();

            try
            {
                using var document = PdfDocument.Open(filePath);

                string fullText = string.Join(
    Environment.NewLine,
    document.GetPages()
        .Select(page =>
            ContentOrderTextExtractor.GetText(page)));

                if (string.IsNullOrWhiteSpace(fullText))
                {
                    result.TextExtracted = false;
                    result.ErrorMessage =
                        "No machine-readable text could be extracted from the PDF.";

                    return result;
                }

                result.TextExtracted = true;

                result.ExtractedPONumber =
                    ExtractPONumber(fullText);

                result.ExtractedAmount =
                    ExtractAmount(fullText);

                return result;
            }
            catch (Exception ex)
            {
                result.TextExtracted = false;

                result.ErrorMessage =
                    $"PDF analysis failed: {ex.Message}";

                return result;
            }
        }

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

            foreach (string pattern in patterns)
            {
                Match match = Regex.Match(
                    text,
                    pattern,
                    RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    return match.Groups[1]
                        .Value
                        .Trim()
                        .TrimEnd('.', ',', ';');
                }
            }

            return null;
        }

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

            foreach (string pattern in patterns)
            {
                Match match = Regex.Match(
                    text,
                    pattern,
                    RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    continue;
                }

                string amountText =
                    match.Groups[1]
                        .Value
                        .Replace(",", "")
                        .Replace(" ", "")
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Trim();

                if (decimal.TryParse(
                    amountText,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal amount))
                {
                    return amount;
                }
            }

            return null;
        }
    }
}