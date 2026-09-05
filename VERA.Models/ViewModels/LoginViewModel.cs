using System.ComponentModel.DataAnnotations;

namespace VERA.Models.ViewModels
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "Business email is required.")]
		[EmailAddress(ErrorMessage = "Please enter a valid email address.")]
		[Display(Name = "Business email")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required.")]
		[DataType(DataType.Password)]
		public string Password { get; set; } = string.Empty;

		[Display(Name = "Remember me")]
		public bool RememberMe { get; set; }
	}
}
