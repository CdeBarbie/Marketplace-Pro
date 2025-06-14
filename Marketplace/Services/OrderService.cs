using MarketPlace.Data;
using MarketPlace.Models;
using MarketPlace.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> CreateOrderAsync(int customerId, CheckoutViewModel checkoutModel)
        {
            var result = new ServiceResult();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate customer exists
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    result.Errors.Add("Customer not found");
                    return result;
                }

                // Validate cart items and check stock
                var orderItems = new List<OrderItem>();
                decimal totalAmount = 0;

                foreach (var cartItem in checkoutModel.CartItems)
                {
                    var product = await _context.Products.FindAsync(cartItem.ProductId);
                    if (product == null)
                    {
                        result.Errors.Add($"Product {cartItem.ProductName} not found");
                        return result;
                    }

                    if (product.StockQuantity < cartItem.Quantity)
                    {
                        result.Errors.Add($"Insufficient stock for {product.Name}. Available: {product.StockQuantity}");
                        return result;
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = product.ProductId,
                        Quantity = cartItem.Quantity,
                        Price = product.Price // Use current price
                    };

                    orderItems.Add(orderItem);
                    totalAmount += orderItem.Price * orderItem.Quantity;

                    // Update stock
                    product.StockQuantity -= cartItem.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                }

                // Create order
                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    TotalAmount = totalAmount,
                    ShippingAddress = checkoutModel.ShippingAddress,
                    OrderItems = orderItems
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create payment record
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = totalAmount,
                    PaymentMethod = checkoutModel.PaymentMethod,
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Pending,
                    TransactionId = $"TXN_{order.OrderId}_{DateTime.UtcNow.Ticks}"
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Simulate payment processing
                await ProcessPaymentAsync(order.OrderId, checkoutModel.PaymentMethod);

                await transaction.CommitAsync();

                result.Success = true;
                result.Message = "Order created successfully";
                result.OrderId = order.OrderId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Errors.Add($"Error creating order: {ex.Message}");
            }

            return result;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId, int customerId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customerId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByVendorAsync(int vendorId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .Where(o => o.OrderItems.Any(oi => oi.Product.VendorId == vendorId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<ServiceResult> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var result = new ServiceResult();

            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    result.Errors.Add("Order not found");
                    return result;
                }

                order.Status = status;
                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = "Order status updated successfully";
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error updating order status: {ex.Message}");
            }

            return result;
        }

        public async Task<ServiceResult> ProcessPaymentAsync(int orderId, PaymentMethod paymentMethod)
        {
            var result = new ServiceResult();

            try
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.OrderId == orderId);

                if (payment == null)
                {
                    result.Errors.Add("Payment record not found");
                    return result;
                }

                // Simulate payment processing
                await Task.Delay(1000); // Simulate processing time

                // For demo purposes, assume payment is successful
                payment.Status = PaymentStatus.Completed;
                payment.PaymentDate = DateTime.UtcNow;

                // Update order status
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Processing;
                }

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = "Payment processed successfully";
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error processing payment: {ex.Message}");
            }

            return result;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);
        }
    }
}
