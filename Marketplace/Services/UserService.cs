using MarketPlace.Data;
using MarketPlace.Models;
using MarketPlace.ViewModels;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace MarketPlace.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Customer)
                .Include(u => u.Vendor)
                .FirstOrDefaultAsync(u => u.Username == username && u.Status == UserStatus.Active);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        public async Task<ServiceResult> RegisterAsync(RegisterViewModel model)
        {
            var result = new ServiceResult();

            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                result.Errors.Add("Username already exists");
                return result;
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                result.Errors.Add("Email already exists");
                return result;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create user
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = Enum.Parse<UserRole>(model.Role),
                    Status = UserStatus.Active
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create role-specific record
                if (user.Role == UserRole.Customer)
                {
                    var customer = new Customer
                    {
                        UserId = user.UserId,
                        Address = model.Address ?? string.Empty,
                        PhoneNumber = model.PhoneNumber ?? string.Empty
                    };
                    _context.Customers.Add(customer);
                }
                else if (user.Role == UserRole.Vendor)
                {
                    var vendor = new Vendor
                    {
                        UserId = user.UserId,
                        BusinessName = model.BusinessName ?? string.Empty,
                        BusinessDescription = model.BusinessDescription,
                        ApprovalStatus = ApprovalStatus.Pending
                    };
                    _context.Vendors.Add(vendor);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Success = true;
                result.Message = "Registration successful";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Errors.Add("Registration failed: " + ex.Message);
            }

            return result;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Customer)
                .Include(u => u.Vendor)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Customer)
                .Include(u => u.Vendor)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<ServiceResult> UpdateUserAsync(User user)
        {
            var result = new ServiceResult();
            try
            {
                user.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                result.Success = true;
                result.Message = "User updated successfully";
            }
            catch (Exception ex)
            {
                result.Errors.Add("Update failed: " + ex.Message);
            }
            return result;
        }

        public async Task<ServiceResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var result = new ServiceResult();
            var user = await GetUserByIdAsync(userId);

            if (user == null)
            {
                result.Errors.Add("User not found");
                return result;
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                result.Errors.Add("Current password is incorrect");
                return result;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            return await UpdateUserAsync(user);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Customer)
                .Include(u => u.Vendor)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<ServiceResult> SuspendUserAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResult { Errors = { "User not found" } };
            }

            user.Status = UserStatus.Suspended;
            return await UpdateUserAsync(user);
        }

        public async Task<ServiceResult> ActivateUserAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResult { Errors = { "User not found" } };
            }

            user.Status = UserStatus.Active;
            return await UpdateUserAsync(user);
        }
    }
}
