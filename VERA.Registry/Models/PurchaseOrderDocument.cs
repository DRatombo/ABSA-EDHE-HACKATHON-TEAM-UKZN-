using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VERA.Registry.Models
{
    public class PurchaseOrderDocument
    {
        public int PurchaseOrderDocumentId { get; set; }

        [Required]
        public int RegisteredPurchaseOrderId { get; set; }

        [ValidateNever]
        public RegisteredPurchaseOrder? RegisteredPurchaseOrder { get; set; }

        [Required]
        [Display(Name = "Original File Name")]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Stored File Name")]
        public string StoredFileName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Storage Path")]
        public string StoragePath { get; set; } = string.Empty;

        [Required]
        [Display(Name = "SHA-256 Document Hash")]
        public string DocumentHash { get; set; } = string.Empty;

        public int VersionNumber { get; set; } = 1;

        public bool IsCurrentVersion { get; set; } = true;

        [Display(Name = "Extracted PO Number")]
        public string? ExtractedPONumber { get; set; }

        [Display(Name = "Extracted Amount")]
        public decimal? ExtractedAmount { get; set; }

        public bool? PONumberMatchesRegistry { get; set; }

        public bool? AmountMatchesRegistry { get; set; }

        [Display(Name = "Document Analysis Result")]
        public string AnalysisResult { get; set; } = "NOT_ANALYSED";

        public string? AnalysisNotes { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}