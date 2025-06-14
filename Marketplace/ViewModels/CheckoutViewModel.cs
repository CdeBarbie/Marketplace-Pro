using MarketPlace.Models;
using System.ComponentModel.DataAnnotations;

namespace MarketPlace.ViewModels
{
    public class CheckoutViewModel
    {
        public IEnumerable<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        [Required]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        // Credit Card fields
        [Display(Name = "Card Number")]
        public string? CardNumber { get; set; }

        [Display(Name = "Expiry Date")]
        public string? ExpiryDate { get; set; }

        [Display(Name = "CVV")]
        public string? CVV { get; set; }

        [Display(Name = "Cardholder Name")]
        public string? CardholderName { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal TaxAmount => TotalAmount * 0.08m; // 8% tax
        public decimal GrandTotal => TotalAmount + TaxAmount;
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public decimal Subtotal => Price * Quantity;
    }
}
