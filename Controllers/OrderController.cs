using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AlaSaree3.Models;
using AlaSaree3.Services.Interfaces;
using AlaSaree3.ViewModels.Checkout;

namespace AlaSaree3.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);

            if (!cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add products before checking out.";
                return RedirectToAction("Index", "Cart");
            }

            if (cart.HasInvalidItems)
            {
                TempData["Error"] = "Some items in your cart exceed available stock. Please adjust quantities before checkout.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.FindByIdAsync(customerId);
            var viewModel = new CheckoutViewModel
            {
                PhoneNumber = user?.PhoneNumber ?? string.Empty,
                Cart = cart
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (!ModelState.IsValid)
            {
                model.Cart = await _cartService.GetCartByCustomerIdAsync(customerId);
                return View(model);
            }

            var result = await _orderService.CheckoutAsync(customerId, model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Checkout failed. Please try again.");
                model.Cart = await _cartService.GetCartByCustomerIdAsync(customerId);
                return View(model);
            }

            TempData["Success"] = "Your order has been placed successfully!";
            return RedirectToAction(nameof(Confirmation), new { id = result.OrderId });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var orderDetails = await _orderService.GetOrderDetailsAsync(id, customerId, isSeller: false, isAdmin: false);

            if (orderDetails == null)
            {
                return NotFound();
            }

            var viewModel = new OrderConfirmationViewModel
            {
                OrderId = orderDetails.Order.Id,
                OrderDate = orderDetails.Order.OrderDate,
                TotalAmount = orderDetails.Order.TotalAmount,
                Status = orderDetails.Order.Status,
                ShippingAddress = orderDetails.Order.ShippingAddress,
                City = orderDetails.Order.City,
                PostalCode = orderDetails.Order.PostalCode,
                PhoneNumber = orderDetails.Order.PhoneNumber,
                Items = orderDetails.Items.ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("/Order")]
        [Route("/Order/Index")]
        [Route("/Order/MyOrders")]
        public async Task<IActionResult> MyOrders()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var orders = await _orderService.GetCustomerOrdersAsync(customerId);
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var orderDetails = await _orderService.GetOrderDetailsAsync(id, customerId, isSeller: false, isAdmin: false);

            if (orderDetails == null)
            {
                return NotFound();
            }

            return View(orderDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _orderService.CancelOrderAsync(id, customerId, isAdmin: false);

            if (result.Success)
            {
                TempData["Success"] = "Order cancelled successfully and inventory has been replenished.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Could not cancel this order.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
