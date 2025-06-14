using MarketPlace.Models;
using MarketPlace.ViewModels;

namespace MarketPlace.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<ServiceResult> RegisterAsync(RegisterViewModel model);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<ServiceResult> UpdateUserAsync(User user);
        Task<ServiceResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<ServiceResult> SuspendUserAsync(int userId);
        Task<ServiceResult> ActivateUserAsync(int userId);
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public int? OrderId { get; set; }
    }
}
