using MarketPlace.Data;
using MarketPlace.Models;
using MarketPlace.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;

        public HomeController(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public async Task<IActionResult> Index(string searchTerm = "", int categoryId = 0, int page = 1, int pageSize = 12)
        {
            try
            {
                var products = await _productService.GetProductsAsync(searchTerm, categoryId, page, pageSize);
                var categories = await _context.Categories.ToListAsync();

                ViewBag.Categories = categories;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.CategoryId = categoryId;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                return View(products);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in HomeController.Index: {ex.Message}");

                // Return empty collections to prevent crashes
                ViewBag.Categories = new List<Category>();
                ViewBag.SearchTerm = searchTerm;
                ViewBag.CategoryId = categoryId;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                return View(new List<Product>());
            }
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Vendor)
                    .ThenInclude(v => v.User)
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                    .ThenInclude(r => r.Customer)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProductDetails: {ex.Message}");
                return NotFound();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
