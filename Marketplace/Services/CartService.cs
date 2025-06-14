using MarketPlace.Data;
using MarketPlace.Models;
using MarketPlace.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<CartItemViewModel>> GetCartItemsAsync(int userId)
        {
            // For this implementation, we'll use session-based cart
            // In a production system, you might want to store cart in database
            var cartItems = GetCartFromSession();
            var cartViewModels = new List<CartItemViewModel>();

            foreach (var item in cartItems)
            {
                var product = await _context.Products
                    .Include(p => p.Vendor)
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                if (product != null)
                {
                    cartViewModels.Add(new CartItemViewModel
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        Quantity = item.Quantity,
                        ImageUrl = product.ImageUrl,
                        VendorName = product.Vendor.BusinessName
                    });
                }
            }

            return cartViewModels;
        }

        public async Task<ServiceResult> AddToCartAsync(int userId, int productId, int quantity)
        {
            var result = new ServiceResult();

            try
            {
                // Check if product exists and has sufficient stock
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.Status == ProductStatus.Active);

                if (product == null)
                {
                    result.Errors.Add("Product not found or not available");
                    return result;
                }

                if (product.StockQuantity < quantity)
                {
                    result.Errors.Add("Insufficient stock available");
                    return result;
                }

                // Get current cart
                var cartItems = GetCartFromSession();
                var existingItem = cartItems.FirstOrDefault(c => c.ProductId == productId);

                if (existingItem != null)
                {
                    // Update quantity if item already exists
                    var newQuantity = existingItem.Quantity + quantity;
                    if (product.StockQuantity < newQuantity)
                    {
                        result.Errors.Add("Cannot add more items. Insufficient stock available");
                        return result;
                    }
                    existingItem.Quantity = newQuantity;
                }
                else
                {
                    // Add new item to cart
                    cartItems.Add(new CartItem
                    {
                        ProductId = productId,
                        Quantity = quantity
                    });
                }

                // Save cart to session
                SaveCartToSession(cartItems);

                result.Success = true;
                result.Message = "Product added to cart successfully";
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error adding product to cart: {ex.Message}");
            }

            return result;
        }

        public async Task<ServiceResult> UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var result = new ServiceResult();

            try
            {
                if (quantity <= 0)
                {
                    return await RemoveFromCartAsync(userId, productId);
                }

                // Check product stock
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                {
                    result.Errors.Add("Product not found");
                    return result;
                }

                if (product.StockQuantity < quantity)
                {
                    result.Errors.Add("Insufficient stock available");
                    return result;
                }

                // Update cart
                var cartItems = GetCartFromSession();
                var existingItem = cartItems.FirstOrDefault(c => c.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity = quantity;
                    SaveCartToSession(cartItems);
                    result.Success = true;
                    result.Message = "Cart updated successfully";
                }
                else
                {
                    result.Errors.Add("Item not found in cart");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error updating cart: {ex.Message}");
            }

            return result;
        }

        public async Task<ServiceResult> RemoveFromCartAsync(int userId, int productId)
        {
            var result = new ServiceResult();

            try
            {
                var cartItems = GetCartFromSession();
                var itemToRemove = cartItems.FirstOrDefault(c => c.ProductId == productId);

                if (itemToRemove != null)
                {
                    cartItems.Remove(itemToRemove);
                    SaveCartToSession(cartItems);
                    result.Success = true;
                    result.Message = "Item removed from cart";
                }
                else
                {
                    result.Errors.Add("Item not found in cart");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error removing item from cart: {ex.Message}");
            }

            return result;
        }

        public async Task<ServiceResult> ClearCartAsync(int userId)
        {
            var result = new ServiceResult();

            try
            {
                SaveCartToSession(new List<CartItem>());
                result.Success = true;
                result.Message = "Cart cleared successfully";
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error clearing cart: {ex.Message}");
            }

            return result;
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            var cartItems = GetCartFromSession();
            return cartItems.Sum(c => c.Quantity);
        }

        private List<CartItem> GetCartFromSession()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return new List<CartItem>();

            var cartJson = session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItem>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        private void SaveCartToSession(List<CartItem> cartItems)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            var cartJson = System.Text.Json.JsonSerializer.Serialize(cartItems);
            session.SetString("Cart", cartJson);
        }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
