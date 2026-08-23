using System.Diagnostics;
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

        public HomeController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, string? sortBy, int page = 1)
        {
            var viewModel = await _productService.GetFilteredProductsAsync(search, categoryId, sortBy, page, 12);
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
