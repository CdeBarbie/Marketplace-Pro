using MarketPlace.Models;
using MarketPlace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketPlace.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class VendorController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public VendorController(IProductService productService, IOrderService orderService, IUserService userService)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _userService.GetUserByIdAsync(userId);

                if (user?.Vendor == null)
                {
                    TempData["ErrorMessage"] = "Vendor profile not found. Please contact support.";
                    return RedirectToAction("Login", "Account");
                }

                var products = await _productService.GetProductsByVendorAsync(user.Vendor.VendorId);
                var orders = await _orderService.GetOrdersByVendorAsync(user.Vendor.VendorId);

                ViewBag.TotalProducts = products.Count();
                ViewBag.TotalRevenue = orders.Sum(o => o.TotalAmount);
                ViewBag.TotalOrders = orders.Count();

                return View(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Vendor Dashboard: {ex.Message}");
                TempData["ErrorMessage"] = "Error loading dashboard. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Products()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _userService.GetUserByIdAsync(userId);

                if (user?.Vendor == null)
                {
                    TempData["ErrorMessage"] = "Vendor profile not found.";
                    return RedirectToAction("Login", "Account");
                }

                var products = await _productService.GetProductsByVendorAsync(user.Vendor.VendorId);
                return View(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading vendor products: {ex.Message}");
                TempData["ErrorMessage"] = "Error loading products.";
                return View(new List<Product>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            try
            {
                var categories = await _productService.GetCategoriesAsync();
                ViewBag.Categories = categories;
                return View(new Product());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading add product page: {ex.Message}");
                ViewBag.Categories = new List<Category>();
                return View(new Product());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product)
        {
            try
            {
                // Remove VendorId from ModelState since we'll set it manually
                ModelState.Remove("VendorId");
                ModelState.Remove("Vendor");
                ModelState.Remove("Category");
                ModelState.Remove("OrderItems");
                ModelState.Remove("Reviews");

                if (ModelState.IsValid)
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    var user = await _userService.GetUserByIdAsync(userId);

                    if (user?.Vendor == null)
                    {
                        TempData["ErrorMessage"] = "Vendor profile not found.";
                        return RedirectToAction("Login", "Account");
                    }

                    // Set the vendor ID
                    product.VendorId = user.Vendor.VendorId;

                    var result = await _productService.CreateProductAsync(product);

                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = result.Message;
                        return RedirectToAction("Products");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                else
                {
                    // Log validation errors
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding product: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while adding the product. Please try again.");
            }

            // Reload categories if there's an error
            try
            {
                var categories = await _productService.GetCategoriesAsync();
                ViewBag.Categories = categories;
            }
            catch
            {
                ViewBag.Categories = new List<Category>();
            }

            return View(product);
        }

        public async Task<IActionResult> Orders()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _userService.GetUserByIdAsync(userId);

                if (user?.Vendor == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var orders = await _orderService.GetOrdersByVendorAsync(user.Vendor.VendorId);
                return View(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading vendor orders: {ex.Message}");
                return View(new List<Order>());
            }
        }
    }
}
