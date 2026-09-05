using System.ComponentModel.DataAnnotations;

namespace VERA.Registry.Models.ViewModels
{
    public class VerifyPORequest
    {
        [Required]
        [Display(Name = "VERA PO ID")]
        public string VeraPOId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "PO Number")]
        public string PONumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
    }
}