using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VERA.Registry.Models.ViewModels
{
    public class UploadPODocumentViewModel
    {
        [Required]
        public int RegisteredPurchaseOrderId { get; set; }

        [Required]
        [Display(Name = "Purchase Order PDF")]
        public IFormFile? Document { get; set; }
    }
}