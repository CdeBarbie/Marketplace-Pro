using MarketPlace.Models;

namespace MarketPlace.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProductsAsync(string searchTerm = "", int categoryId = 0, int page = 1, int pageSize = 12);
        Task<Product?> GetProductByIdAsync(int productId);
        Task<IEnumerable<Product>> GetProductsByVendorAsync(int vendorId);
        Task<ServiceResult> CreateProductAsync(Product product);
        Task<ServiceResult> UpdateProductAsync(Product product);
        Task<ServiceResult> DeleteProductAsync(int productId);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<ServiceResult> UpdateStockAsync(int productId, int newStock);
    }
}
