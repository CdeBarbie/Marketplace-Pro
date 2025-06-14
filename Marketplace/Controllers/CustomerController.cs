using MarketPlace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketPlace.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public CustomerController(IOrderService orderService, IUserService userService)
        {
            _orderService = orderService;
            _userService = userService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userService.GetUserByIdAsync(userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetOrdersByCustomerAsync(user.Customer.CustomerId);

            ViewBag.TotalOrders = orders.Count();
            ViewBag.TotalSpent = orders.Sum(o => o.TotalAmount);

            return View(orders);
        }

        public async Task<IActionResult> Orders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userService.GetUserByIdAsync(userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetOrdersByCustomerAsync(user.Customer.CustomerId);
            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userService.GetUserByIdAsync(userId);

            if (user?.Customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(id, user.Customer.CustomerId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
