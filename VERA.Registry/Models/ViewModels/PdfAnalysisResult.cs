namespace VERA.Registry.Models.ViewModels
{
    // Stores the result of reading an uploaded PDF
    public class PdfAnalysisResult
    {
        // True when readable text was found in the PDF
        public bool TextExtracted { get; set; }

        // True when the document looks like a purchase order
        public bool LooksLikePurchaseOrder { get; set; }

        // PO number found in the document
        public string? ExtractedPONumber { get; set; }

        // PO amount found in the document
        public decimal? ExtractedAmount { get; set; }

        // Message shown if analysis fails
        public string? ErrorMessage { get; set; }
    }
}