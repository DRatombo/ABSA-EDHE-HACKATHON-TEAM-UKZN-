using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace VERA.Web.Models
{
    // Holds the information entered on the new opportunity page
    public class NewOpportunityViewModel
    {
        // Purchase order number
        [Required]
        public string PurchaseOrderNumber { get; set; } = string.Empty;

        // Purchase order value
        [Required]
        public decimal PurchaseOrderValue { get; set; }

        // Date the purchase order was issued
        [Required]
        public DateTime IssueDate { get; set; }

        // Date the opportunity must be delivered
        [Required]
        public DateTime DeliveryDate { get; set; }

        // Short description of the opportunity
        [Required]
        public string OpportunityDescription { get; set; } = string.Empty;


        // Buyer name
        [Required]
        public string BuyerName { get; set; } = string.Empty;

        // Buyer contact person
        public string? BuyerContactPerson { get; set; }

        // Buyer email
        public string? BuyerEmail { get; set; }

        // Buyer phone number
        public string? BuyerPhone { get; set; }

        // Buyer or tender reference
        public string? BuyerReference { get; set; }


        // Purchase order uploaded by the SME
        [Required]
        public IFormFile? PurchaseOrderFile { get; set; }

        // Supplier quote uploaded by the SME
        [Required]
        public IFormFile? SupplierQuoteFile { get; set; }

        // Optional tax document
        public IFormFile? TaxComplianceFile { get; set; }

        // Optional authority document
        public IFormFile? AuthorityFile { get; set; }

        // Previous fulfilment evidence
        public List<IFormFile>? CapacityEvidenceFiles { get; set; }

        // Other supporting documents
        public List<IFormFile>? AdditionalDocuments { get; set; }


        // Estimated cost to fulfil the PO
        [Required]
        public decimal FulfilmentCost { get; set; }

        // Amount the SME can contribute
        public decimal SMEContribution { get; set; }

        // Main supplier for the opportunity
        public string? PrimarySupplier { get; set; }


        // SME must accept the declaration
        [Range(
            typeof(bool),
            "true",
            "true",
            ErrorMessage = "Please accept the declaration.")]
        public bool DeclarationAccepted { get; set; }
    }
}