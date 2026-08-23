using AlaSaree3.ViewModels.Cart;

namespace AlaSaree3.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartByCustomerIdAsync(string customerId);
        Task<(bool Success, string? ErrorMessage)> AddToCartAsync(string customerId, int productId, int quantity);
        Task<(bool Success, string? ErrorMessage)> UpdateQuantityAsync(string customerId, int cartItemId, int quantity);
        Task<(bool Success, string? ErrorMessage)> RemoveFromCartAsync(string customerId, int cartItemId);
        Task<bool> ClearCartAsync(string customerId);
        Task<int> GetCartItemCountAsync(string customerId);
    }
}
