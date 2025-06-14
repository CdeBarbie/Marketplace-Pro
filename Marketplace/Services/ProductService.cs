using MarketPlace.Data;
using MarketPlace.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync(string searchTerm = "", int categoryId = 0, int page = 1, int pageSize = 12)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Vendor)
                    .ThenInclude(v => v.User)
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                    .Where(p => p.Status == ProductStatus.Active);

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
                }

                // Apply category filter
                if (categoryId > 0)
                {
                    query = query.Where(p => p.CategoryId == categoryId);
                }

                // Apply pagination
                query = query.Skip((page - 1) * pageSize).Take(pageSize);

                var products = await query.OrderBy(p => p.Name).ToListAsync();
                Console.WriteLine($"Retrieved {products.Count} products from database");
                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProductsAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<Product>();
            }
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Vendor)
                    .ThenInclude(v => v.User)
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                    .ThenInclude(r => r.Customer)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(p => p.ProductId == productId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProductByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsByVendorAsync(int vendorId)
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                    .Where(p => p.VendorId == vendorId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                Console.WriteLine($"Retrieved {products.Count} products for vendor {vendorId}");
                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProductsByVendorAsync: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<ServiceResult> CreateProductAsync(Product product)
        {
            var result = new ServiceResult();

            try
            {
                Console.WriteLine($"Creating product: {product.Name} for vendor {product.VendorId}");

                // Validate vendor exists and is approved
                var vendor = await _context.Vendors
                    .FirstOrDefaultAsync(v => v.VendorId == product.VendorId && v.ApprovalStatus == ApprovalStatus.Approved);

                if (vendor == null)
                {
                    result.Errors.Add("Vendor not found or not approved");
                    Console.WriteLine($"Vendor {product.VendorId} not found or not approved");
                    return result;
                }

                // Validate category exists
                var category = await _context.Categories.FindAsync(product.CategoryId);
                if (category == null)
                {
                    result.Errors.Add("Category not found");
                    Console.WriteLine($"Category {product.CategoryId} not found");
                    return result;
                }

                // Set timestamps and status
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;
                product.Status = ProductStatus.Active; // Change to Active for immediate listing

                // Add to context
                _context.Products.Add(product);

                // Save changes
                var saved = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChanges returned: {saved}");

                if (saved > 0)
                {
                    result.Success = true;
                    result.Message = "Product created successfully";
                    Console.WriteLine($"Product {product.Name} created with ID: {product.ProductId}");
                }
                else
                {
                    result.Errors.Add("Failed to save product to database");
                    Console.WriteLine("SaveChanges returned 0 - no changes saved");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                result.Errors.Add($"Error creating product: {ex.Message}");
            }

            return result;
        }

        public async Task<ServiceResult> UpdateProductAsync(Product product)
        {
            var result = new ServiceResult();

            try
            {
                var existingProduct = await _context.Products.FindAsync(product.ProductId);
                if (existingProduct == null)
                {
                    result.Errors.Add("Product not found");
                    return result;
                }

                // Update properties
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = "Product updated successfully";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                result.Errors.Add($"Error updating product: {ex.Message}");
            }

            return result;
        }

        public async Task<ServiceResult> DeleteProductAsync(int productId)
        {
            var result = new ServiceResult();

            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    result.Errors.Add("Product not found");
                    return result;
                }

                // Check if product has any orders
                var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == productId);
                if (hasOrders)
                {
                    // Don't delete, just mark as inactive
                    product.Status = ProductStatus.Inactive;
                    result.Message = "Product marked as inactive (has existing orders)";
                }
                else
                {
                    _context.Products.Remove(product);
                    result.Message = "Product deleted successfully";
                }

                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product: {ex.Message}");
                result.Errors.Add($"Error deleting product: {ex.Message}");
            }

            return result;
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8)
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Vendor)
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                    .Where(p => p.Status == ProductStatus.Active)
                    .OrderByDescending(p => p.Reviews.Count)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFeaturedProductsAsync: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                Console.WriteLine($"Retrieved {categories.Count} categories");
                return categories;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCategoriesAsync: {ex.Message}");
                return new List<Category>();
            }
        }

        public async Task<ServiceResult> UpdateStockAsync(int productId, int newStock)
        {
            var result = new ServiceResult();

            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    result.Errors.Add("Product not found");
                    return result;
                }

                product.StockQuantity = newStock;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = "Stock updated successfully";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating stock: {ex.Message}");
                result.Errors.Add($"Error updating stock: {ex.Message}");
            }

            return result;
        }
    }
}
