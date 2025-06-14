using MarketPlace.ViewModels;

namespace MarketPlace.Services
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemViewModel>> GetCartItemsAsync(int userId);
        Task<ServiceResult> AddToCartAsync(int userId, int productId, int quantity);
        Task<ServiceResult> UpdateQuantityAsync(int userId, int productId, int quantity);
        Task<ServiceResult> RemoveFromCartAsync(int userId, int productId);
        Task<ServiceResult> ClearCartAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
    }
}
