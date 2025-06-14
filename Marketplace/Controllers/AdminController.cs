using MarketPlace.Data;
using MarketPlace.Models;
using MarketPlace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public AdminController(ApplicationDbContext context, IUserService userService,
                              IProductService productService, IOrderService orderService)
        {
            _context = context;
            _userService = userService;
            _productService = productService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalVendors = await _context.Vendors.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _orderService.GetTotalRevenueAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalVendors = totalVendors;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;

            return View();
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> Vendors()
        {
            var vendors = await _context.Vendors
                .Include(v => v.User)
                .OrderByDescending(v => v.RegistrationDate)
                .ToListAsync();
            return View(vendors);
        }

        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .Include(p => p.Vendor)
                .ThenInclude(v => v.User)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveVendor(int vendorId)
        {
            var vendor = await _context.Vendors.FindAsync(vendorId);
            if (vendor != null)
            {
                vendor.ApprovalStatus = ApprovalStatus.Approved;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vendor approved successfully!";
            }
            return RedirectToAction("Vendors");
        }

        [HttpPost]
        public async Task<IActionResult> RejectVendor(int vendorId)
        {
            var vendor = await _context.Vendors.FindAsync(vendorId);
            if (vendor != null)
            {
                vendor.ApprovalStatus = ApprovalStatus.Rejected;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vendor rejected!";
            }
            return RedirectToAction("Vendors");
        }

        [HttpPost]
        public async Task<IActionResult> ApproveProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Status = ProductStatus.Active;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product approved successfully!";
            }
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> SuspendUser(int userId)
        {
            var result = await _userService.SuspendUserAsync(userId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "User suspended successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Errors);
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateUser(int userId)
        {
            var result = await _userService.ActivateUserAsync(userId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "User activated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Errors);
            }
            return RedirectToAction("Users");
        }
    }
}
