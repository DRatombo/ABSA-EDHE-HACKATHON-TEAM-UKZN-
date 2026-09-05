using System.ComponentModel.DataAnnotations;

namespace VERA.Web.ViewModels
{
	public class RegisterViewModel
	{
		// =====================================================
		// BUSINESS DETAILS
		// =====================================================

		[Required(ErrorMessage = "Business legal name is required.")]
		[Display(Name = "Registered business name")]
		public string BusinessName { get; set; } = string.Empty;


		[Display(Name = "Trading name")]
		public string? TradingName { get; set; }


		[Required(ErrorMessage = "Company registration number is required.")]
		[Display(Name = "Company registration number")]
		public string RegistrationNumber { get; set; } = string.Empty;


		[Required(ErrorMessage = "Please select an entity type.")]
		[Display(Name = "Business type")]
		public string BusinessType { get; set; } = string.Empty;


		[Required(ErrorMessage = "Please select an industry.")]
		[Display(Name = "Industry / sector")]
		public string Industry { get; set; } = string.Empty;


		[Display(Name = "Years in operation")]
		[Range(0, 100)]
		public int? YearsOperating { get; set; }


		[Display(Name = "Number of employees")]
		[Range(0, 100000)]
		public int? EmployeeCount { get; set; }


		[Display(Name = "VAT number")]
		public string? VatNumber { get; set; }


		[Display(Name = "Tax reference number")]
		public string? TaxReferenceNumber { get; set; }


		[Display(Name = "CSD supplier number")]
		public string? CsdNumber { get; set; }


		[Display(Name = "Business website")]
		[Url(ErrorMessage = "Please enter a valid website address.")]
		public string? Website { get; set; }


		[Display(Name = "Are you an Absa Business customer?")]
		public bool IsAbsaCustomer { get; set; }


		// =====================================================
		// BUSINESS ADDRESS
		// =====================================================

		[Required(ErrorMessage = "Business address is required.")]
		[Display(Name = "Street address")]
		public string StreetAddress { get; set; } = string.Empty;


		[Required(ErrorMessage = "City is required.")]
		public string City { get; set; } = string.Empty;


		[Required(ErrorMessage = "Province is required.")]
		public string Province { get; set; } = string.Empty;


		[Required(ErrorMessage = "Postal code is required.")]
		[Display(Name = "Postal code")]
		public string PostalCode { get; set; } = string.Empty;


		// =====================================================
		// PRIMARY CONTACT
		// =====================================================

		[Required(ErrorMessage = "First name is required.")]
		[Display(Name = "First name")]
		public string FirstName { get; set; } = string.Empty;


		[Required(ErrorMessage = "Surname is required.")]
		public string Surname { get; set; } = string.Empty;


		[Required(ErrorMessage = "Your role is required.")]
		[Display(Name = "Role in the business")]
		public string ContactRole { get; set; } = string.Empty;


		[Required(ErrorMessage = "Email address is required.")]
		[EmailAddress(ErrorMessage = "Please enter a valid email address.")]
		[Display(Name = "Business email")]
		public string Email { get; set; } = string.Empty;


		[Required(ErrorMessage = "Phone number is required.")]
		[Phone(ErrorMessage = "Please enter a valid phone number.")]
		[Display(Name = "Mobile number")]
		public string PhoneNumber { get; set; } = string.Empty;


		// =====================================================
		// SECURITY
		// =====================================================

		[Required(ErrorMessage = "Password is required.")]
		[MinLength(8, ErrorMessage = "Password must contain at least 8 characters.")]
		[DataType(DataType.Password)]
		public string Password { get; set; } = string.Empty;


		[Required(ErrorMessage = "Please confirm your password.")]
		[DataType(DataType.Password)]
		[Compare("Password", ErrorMessage = "The passwords do not match.")]
		[Display(Name = "Confirm password")]
		public string ConfirmPassword { get; set; } = string.Empty;


		[Range(typeof(bool), "true", "true",
			ErrorMessage = "You must confirm that the information provided is accurate.")]
		public bool ConfirmInformation { get; set; }
	}
}