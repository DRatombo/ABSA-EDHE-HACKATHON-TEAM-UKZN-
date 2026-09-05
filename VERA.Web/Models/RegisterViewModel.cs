using System.ComponentModel.DataAnnotations;

namespace VERA.Web.ViewModels
{
    public class RegisterViewModel : IValidatableObject
    {
        // =====================================================
        // ACCOUNT TYPE
        // =====================================================

        [Required(ErrorMessage = "Please choose an account type.")]
        [Display(Name = "Account type")]
        public string AccountType { get; set; } = "SME";


        // =====================================================
        // BUSINESS DETAILS (SME accounts)
        // =====================================================

        [Display(Name = "Registered business name")]
        public string BusinessName { get; set; } = string.Empty;


        [Display(Name = "Trading name")]
        public string? TradingName { get; set; }


        [Display(Name = "Company registration number")]
        public string RegistrationNumber { get; set; } = string.Empty;


        [Display(Name = "Business type")]
        public string BusinessType { get; set; } = string.Empty;


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
        // BUSINESS ADDRESS (SME accounts)
        // =====================================================

        [Display(Name = "Street address")]
        public string StreetAddress { get; set; } = string.Empty;


        public string City { get; set; } = string.Empty;


        public string Province { get; set; } = string.Empty;


        [Display(Name = "Postal code")]
        public string PostalCode { get; set; } = string.Empty;


        // =====================================================
        // FUNDER / INVESTOR DETAILS (Funder accounts)
        // =====================================================

        [Display(Name = "Organisation name")]
        public string OrganisationName { get; set; } = string.Empty;


        [Display(Name = "Organisation type")]
        public string FunderType { get; set; } = string.Empty;


        [Display(Name = "Typical funding range")]
        public string? TypicalFundingRange { get; set; }


        [Display(Name = "Industries of interest")]
        public string? InvestmentFocus { get; set; }


        [Display(Name = "Business website")]
        [Url(ErrorMessage = "Please enter a valid website address.")]
        public string? FunderWebsite { get; set; }


        // =====================================================
        // PRIMARY CONTACT (both account types)
        // =====================================================

        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Surname is required.")]
        public string Surname { get; set; } = string.Empty;


        [Required(ErrorMessage = "Your role is required.")]
        [Display(Name = "Role")]
        public string ContactRole { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Mobile number")]
        public string PhoneNumber { get; set; } = string.Empty;


        // =====================================================
        // SECURITY (both account types)
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


        // =====================================================
        // CONDITIONAL VALIDATION
        // =====================================================
        //
        // SME-only and Funder-only fields aren't marked [Required]
        // directly, because only one set of them is ever visible on
        // screen at a time (the JS on the Register page shows/hides
        // them based on AccountType). Enforcing both sets at once
        // would make it impossible to submit either form. Instead,
        // only the fields relevant to the chosen AccountType are
        // required here.

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsSme)
            {
                if (string.IsNullOrWhiteSpace(BusinessName))
                    yield return new ValidationResult("Business legal name is required.", new[] { nameof(BusinessName) });

                if (string.IsNullOrWhiteSpace(RegistrationNumber))
                    yield return new ValidationResult("Company registration number is required.", new[] { nameof(RegistrationNumber) });

                if (string.IsNullOrWhiteSpace(BusinessType))
                    yield return new ValidationResult("Please select an entity type.", new[] { nameof(BusinessType) });

                if (string.IsNullOrWhiteSpace(Industry))
                    yield return new ValidationResult("Please select an industry.", new[] { nameof(Industry) });

                if (string.IsNullOrWhiteSpace(StreetAddress))
                    yield return new ValidationResult("Business address is required.", new[] { nameof(StreetAddress) });

                if (string.IsNullOrWhiteSpace(City))
                    yield return new ValidationResult("City is required.", new[] { nameof(City) });

                if (string.IsNullOrWhiteSpace(Province))
                    yield return new ValidationResult("Province is required.", new[] { nameof(Province) });

                if (string.IsNullOrWhiteSpace(PostalCode))
                    yield return new ValidationResult("Postal code is required.", new[] { nameof(PostalCode) });
            }
            else if (IsFunder)
            {
                if (string.IsNullOrWhiteSpace(OrganisationName))
                    yield return new ValidationResult("Organisation name is required.", new[] { nameof(OrganisationName) });

                if (string.IsNullOrWhiteSpace(FunderType))
                    yield return new ValidationResult("Please select an organisation type.", new[] { nameof(FunderType) });
            }
        }

        public bool IsSme => string.Equals(AccountType, "SME", StringComparison.OrdinalIgnoreCase);

        public bool IsFunder => string.Equals(AccountType, "Funder", StringComparison.OrdinalIgnoreCase);
    }
}
