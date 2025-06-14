using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketPlace.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Role")]
        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public UserStatus Status { get; set; } = UserStatus.Active;

        // Navigation properties
        public Customer? Customer { get; set; }
        public Vendor? Vendor { get; set; }
    }

    public enum UserRole
    {
        Customer,
        Vendor,
        Admin
    }

    public enum UserStatus
    {
        Active,
        Suspended,
        Pending
    }

    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }

    public class Vendor
    {
        [Key]
        public int VendorId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; }

        [Display(Name = "Business Description")]
        public string? BusinessDescription { get; set; }

        [Display(Name = "Registration Date")]
        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow.Date;

        [Display(Name = "Approval Status")]
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

        [Display(Name = "Tax ID")]
        public string? TaxId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
