using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AlaSaree3.Services.Interfaces;
using AlaSaree3.ViewModels.Review;

namespace AlaSaree3.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddReviewViewModel model)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please provide a valid rating (1-5) and comment.";
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }

            var result = await _reviewService.AddReviewAsync(customerId, model);

            if (result.Success)
            {
                TempData["Success"] = "Thank you! Your verified purchase review has been posted.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Could not submit review.";
            }

            return RedirectToAction("Details", "Product", new { id = model.ProductId });
        }
    }
}
