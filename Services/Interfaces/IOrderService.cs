using AlaSaree3.Models;
using AlaSaree3.ViewModels.Checkout;
using AlaSaree3.ViewModels.Order;

namespace AlaSaree3.Services.Interfaces
{
    public interface IOrderService
    {
        Task<(bool Success, int? OrderId, string? ErrorMessage)> CheckoutAsync(string customerId, CheckoutViewModel model);
        Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId);
        Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int orderId, string currentUserId, bool isSeller, bool isAdmin);
        Task<(bool Success, string? ErrorMessage)> CancelOrderAsync(int orderId, string currentUserId, bool isAdmin);
        Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string currentUserId, bool isAdmin, bool isSeller);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
    }
}
