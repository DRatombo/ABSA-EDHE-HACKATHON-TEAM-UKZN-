using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace VERA.Web.Models
{
    // Holds everything needed on the Registry verification page
    public class RegistryVerifyViewModel
    {
        // The VERA Registry ID for the purchase order
        [Required]
        [Display(Name = "VERA PO ID")]
        public string VeraPOId { get; set; } = string.Empty;


        // The PO number entered manually or extracted from the PDF
        [Required]
        [Display(Name = "PO Number")]
        public string PONumber { get; set; } = string.Empty;


        // The supplier name linked to the opportunity
        [Required]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; } = string.Empty;


        // The PO amount entered manually or extracted from the PDF
        [Required]
        [Display(Name = "PO Amount")]
        public decimal Amount { get; set; }


        // Optional PDF uploaded by the user
        [Display(Name = "Purchase Order PDF")]
        public IFormFile? Document { get; set; }


        // Used to show whether PDF reading worked
        public bool PdfAnalysed { get; set; }


        // Message shown after trying to read the PDF
        public string? PdfMessage { get; set; }
    }
}