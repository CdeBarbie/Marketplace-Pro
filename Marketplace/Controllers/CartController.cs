using MarketPlace.Services;
using MarketPlace.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketPlace.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartController(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _cartService.AddToCartAsync(userId, productId, quantity);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Product added to cart successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("ProductDetails", "Home", new { id = productId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _cartService.UpdateQuantityAsync(userId, productId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _cartService.RemoveFromCartAsync(userId, productId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var cartItems = await _cartService.GetCartItemsAsync(userId);

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(item => item.Subtotal)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _orderService.CreateOrderAsync(userId, model);

                if (result.Success)
                {
                    await _cartService.ClearCartAsync(userId);
                    TempData["SuccessMessage"] = $"Order placed successfully! Order ID: {result.OrderId}";
                    return RedirectToAction("OrderConfirmation", new { orderId = result.OrderId });
                }

                ModelState.AddModelError(string.Empty, result.Message);
            }

            // Reload cart items if validation fails
            var userIdReload = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            model.CartItems = await _cartService.GetCartItemsAsync(userIdReload);
            return View("Checkout", model);
        }

        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var order = await _orderService.GetOrderByIdAsync(orderId, userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
