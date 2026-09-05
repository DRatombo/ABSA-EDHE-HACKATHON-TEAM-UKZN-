namespace VERA.Registry.Models.ViewModels
{
    public class PdfAnalysisResult
    {
        public bool TextExtracted { get; set; }

        public string? ExtractedPONumber { get; set; }

        public decimal? ExtractedAmount { get; set; }

        public bool PONumberFound =>
            !string.IsNullOrWhiteSpace(ExtractedPONumber);

        public bool AmountFound =>
            ExtractedAmount.HasValue;

        public string? ErrorMessage { get; set; }
    }
}