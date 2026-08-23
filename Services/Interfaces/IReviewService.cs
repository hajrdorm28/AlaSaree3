using AlaSaree3.Models;
using AlaSaree3.ViewModels.Review;

namespace AlaSaree3.Services.Interfaces
{
    public interface IReviewService
    {
        Task<bool> CanUserReviewProductAsync(string customerId, int productId);
        Task<(bool Success, string? ErrorMessage)> AddReviewAsync(string customerId, AddReviewViewModel model);
        Task<IEnumerable<Review>> GetProductReviewsAsync(int productId);
    }
}
