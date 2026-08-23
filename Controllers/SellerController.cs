using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AlaSaree3.Models;
using AlaSaree3.Services.Interfaces;
using AlaSaree3.ViewModels.Product;
using AlaSaree3.ViewModels.Seller;

namespace AlaSaree3.Controllers
{
    public class SellerController : Controller
    {
        private readonly ISellerService _sellerService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IOrderService _orderService;

        public SellerController(
            ISellerService sellerService,
            IProductService productService,
            ICategoryService categoryService,
            IOrderService orderService)
        {
            _sellerService = sellerService;
            _productService = productService;
            _categoryService = categoryService;
            _orderService = orderService;
        }

        // ==========================================
        // SELLER REGISTRATION / REQUEST (Customers)
        // ==========================================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RequestSeller()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (User.IsInRole("Seller"))
            {
                TempData["Info"] = "You are already an approved seller!";
                return RedirectToAction(nameof(Dashboard));
            }

            if (await _sellerService.HasPendingRequestAsync(userId))
            {
                var pending = await _sellerService.GetUserPendingRequestAsync(userId);
                ViewBag.PendingRequest = pending;
                return View("PendingSellerRequest");
            }

            return View(new SellerRequestCreateViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestSeller(SellerRequestCreateViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _sellerService.SubmitRequestAsync(userId, model);
            if (result.Success)
            {
                TempData["Success"] = "Your seller application has been submitted successfully! An administrator will review your application.";
                return RedirectToAction("Profile", "Account");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to submit seller request.");
            return View(model);
        }

        // ==========================================
        // SELLER MANAGEMENT (Approved Sellers Only)
        // ==========================================

        [HttpGet]
        [Route("/Seller")]
        [Route("/Seller/Index")]
        [Route("/Seller/Dashboard")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Dashboard()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var viewModel = await _sellerService.GetSellerDashboardAsync(sellerId);
            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> MyProducts()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var products = await _productService.GetProductsBySellerAsync(sellerId);
            return View(products);
        }

        [HttpGet]
        [Route("/Seller/CreateProduct")]
        [Route("/Seller/AddProduct")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateProduct()
        {
            var categories = await _categoryService.GetAllAsync();
            var viewModel = new ProductCreateViewModel
            {
                Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            };
            return View(viewModel);
        }

        [HttpPost]
        [Route("/Seller/CreateProduct")]
        [Route("/Seller/AddProduct")]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductCreateViewModel model)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllAsync();
                model.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                return View(model);
            }

            // Strictly inject seller ID from authenticated claim
            var result = await _productService.CreateProductAsync(model, sellerId);

            if (result.Success)
            {
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(MyProducts));
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create product.");
            var cats = await _categoryService.GetAllAsync();
            model.Categories = cats.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> EditProduct(int id)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var product = await _productService.GetProductForEditAsync(id, sellerId);

            // CRITICAL OWNERSHIP CHECK
            if (product == null)
            {
                return Forbid();
            }

            var categories = await _categoryService.GetAllAsync();
            var viewModel = new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                AvailableQuantity = product.AvailableQuantity,
                CategoryId = product.CategoryId,
                ExistingImageUrl = product.ImageUrl,
                Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name, Selected = c.Id == product.CategoryId })
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductEditViewModel model)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllAsync();
                model.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                return View(model);
            }

            // CRITICAL OWNERSHIP CHECK inside service
            var result = await _productService.UpdateProductAsync(model, sellerId);

            if (result.Success)
            {
                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(MyProducts));
            }

            if (result.ErrorMessage?.Contains("Unauthorized") == true)
            {
                return Forbid();
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update product.");
            var cats = await _categoryService.GetAllAsync();
            model.Categories = cats.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int productId, int quantity)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _productService.UpdateStockAsync(productId, sellerId, quantity);

            if (result.Success)
            {
                TempData["Success"] = "Stock updated successfully.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to update stock.";
            }

            return RedirectToAction(nameof(MyProducts));
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _productService.DeleteProductAsync(id, sellerId);

            if (result.Success)
            {
                TempData["Success"] = "Product deleted successfully.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Could not delete product.";
            }

            return RedirectToAction(nameof(MyProducts));
        }

        [HttpGet]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> MyOrders()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var viewModel = await _sellerService.GetSellerOrdersAsync(sellerId);
            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var orderDetails = await _orderService.GetOrderDetailsAsync(id, sellerId, isSeller: true, isAdmin: false);

            if (orderDetails == null)
            {
                return Forbid();
            }

            return View(orderDetails);
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _orderService.UpdateOrderStatusAsync(orderId, status, sellerId, isAdmin: false, isSeller: true);

            if (result.Success)
            {
                TempData["Success"] = $"Order #{orderId} status updated to {status}.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Could not update order status.";
            }

            return RedirectToAction(nameof(MyOrders));
        }
    }
}
