using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using AlaSaree3.Models;
using AlaSaree3.Services.Interfaces;
using AlaSaree3.ViewModels.Common;

namespace AlaSaree3.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWishlistService _wishlistService;

        public HomeController(IProductService productService, ICategoryService categoryService, IWishlistService wishlistService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _wishlistService = wishlistService;
        }

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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/Home/StatusCodeHandler")]
        public IActionResult StatusCodeHandler(int code)
        {
            ViewBag.StatusCode = code;
            return code switch
            {
                404 => View("NotFound"),
                403 => View("AccessDenied"),
                _ => View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier })
            };
        }
    }
}
