using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AlaSaree3.Services.Interfaces;

namespace AlaSaree3.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var viewModel = await _wishlistService.GetWishlistByCustomerIdAsync(customerId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int productId, string? returnUrl = null)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            bool isInWishlist = await _wishlistService.IsInWishlistAsync(customerId, productId);

            if (isInWishlist)
            {
                await _wishlistService.RemoveFromWishlistAsync(customerId, productId);
                TempData["Info"] = "Product removed from your wishlist.";
            }
            else
            {
                var result = await _wishlistService.AddToWishlistAsync(customerId, productId);
                if (result.Success)
                {
                    TempData["Success"] = "Product added to your wishlist!";
                }
                else
                {
                    TempData["Error"] = result.ErrorMessage;
                }
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _wishlistService.RemoveFromWishlistAsync(customerId, productId);

            if (result.Success)
            {
                TempData["Success"] = "Product removed from your wishlist.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
