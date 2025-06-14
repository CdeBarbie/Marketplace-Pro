using MarketPlace.Models;
using MarketPlace.ViewModels;

namespace MarketPlace.Services
{
    public interface IOrderService
    {
        Task<ServiceResult> CreateOrderAsync(int customerId, CheckoutViewModel checkoutModel);
        Task<Order?> GetOrderByIdAsync(int orderId, int customerId);
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
        Task<IEnumerable<Order>> GetOrdersByVendorAsync(int vendorId);
        Task<ServiceResult> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<ServiceResult> ProcessPaymentAsync(int orderId, PaymentMethod paymentMethod);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<decimal> GetTotalRevenueAsync();
    }
}
