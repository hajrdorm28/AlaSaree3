using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AlaSaree3.Services.Interfaces;

namespace AlaSaree3.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var cartViewModel = await _cartService.GetCartByCustomerIdAsync(customerId);
            return View(cartViewModel);
        }

        [HttpPost]
        [Route("/Cart/Add")]
        [Route("/Cart/AddToCart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _cartService.AddToCartAsync(customerId, productId, quantity);

            if (result.Success)
            {
                TempData["Success"] = "Product added to your cart successfully!";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to add product to cart.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _cartService.UpdateQuantityAsync(customerId, cartItemId, quantity);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage ?? "Could not update quantity.";
            }
            else
            {
                TempData["Success"] = "Cart updated.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _cartService.RemoveFromCartAsync(customerId, cartItemId);

            if (result.Success)
            {
                TempData["Success"] = "Item removed from cart.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to remove item.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _cartService.ClearCartAsync(customerId);
            TempData["Info"] = "Your cart has been cleared.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId))
            {
                return Json(new { count = 0 });
            }

            int count = await _cartService.GetCartItemCountAsync(customerId);
            return Json(new { count });
        }
    }
}
