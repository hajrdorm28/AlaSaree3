using Microsoft.EntityFrameworkCore;
using AlaSaree3.Data;
using AlaSaree3.Models;
using AlaSaree3.Services.Interfaces;
using AlaSaree3.ViewModels.Review;

namespace AlaSaree3.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanUserReviewProductAsync(string customerId, int productId)
        {
            // Verify product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.SellerId == customerId)
            {
                return false; // Cannot review own product
            }

            // Check if already reviewed
            bool alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.CustomerId == customerId && r.ProductId == productId);

            if (alreadyReviewed)
            {
                return false;
            }

            // Check if customer actually purchased this product in a completed/valid order
            bool hasPurchased = await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.ProductId == productId &&
                               oi.Order.CustomerId == customerId &&
                               oi.Order.Status != OrderStatus.Cancelled);

            return hasPurchased;
        }

        public async Task<(bool Success, string? ErrorMessage)> AddReviewAsync(string customerId, AddReviewViewModel model)
        {
            if (model.Rating < 1 || model.Rating > 5)
            {
                return (false, "Rating must be between 1 and 5 stars.");
            }

            bool eligible = await CanUserReviewProductAsync(customerId, model.ProductId);
            if (!eligible)
            {
                return (false, "You are only allowed to review products you have purchased, and you can only submit one review per product.");
            }

            var review = new Review
            {
                ProductId = model.ProductId,
                CustomerId = customerId,
                Rating = model.Rating,
                Comment = model.Comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId)
        {
            return await _context.Reviews
                .Include(r => r.Customer)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
