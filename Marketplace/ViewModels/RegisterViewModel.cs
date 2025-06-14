using System.ComponentModel.DataAnnotations;

namespace MarketPlace.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Account Type")]
        public string Role { get; set; } = string.Empty;

        // Customer specific fields
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Phone Number")]
        [Phone]
        public string? PhoneNumber { get; set; }

        // Vendor specific fields
        [Display(Name = "Business Name")]
        public string? BusinessName { get; set; }

        [Display(Name = "Business Description")]
        public string? BusinessDescription { get; set; }
    }
}
