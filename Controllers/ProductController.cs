using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using AlaSaree3.Services.Interfaces;

namespace AlaSaree3.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWishlistService _wishlistService;

        public ProductController(IProductService productService, ICategoryService categoryService, IWishlistService wishlistService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _wishlistService = wishlistService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? categoryId, string? sortBy, int page = 1)
        {
            var viewModel = await _productService.GetFilteredProductsAsync(search, categoryId, sortBy, page, 12);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(currentUserId))
            {
                viewModel.WishlistProductIds = await _wishlistService.GetWishlistProductIdsAsync(currentUserId);
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var viewModel = await _productService.GetProductDetailsAsync(id, currentUserId);

            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }
    }
}
