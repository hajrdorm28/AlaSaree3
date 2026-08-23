using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AlaSaree3.Models;
using AlaSaree3.Services.Interfaces;

namespace AlaSaree3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminOrdersController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = _userManager.GetUserId(User)!;
            var orderDetails = await _orderService.GetOrderDetailsAsync(id, currentUserId, isSeller: false, isAdmin: true);

            if (orderDetails == null)
            {
                return NotFound();
            }

            return View(orderDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
        {
            var currentUserId = _userManager.GetUserId(User)!;
            var result = await _orderService.UpdateOrderStatusAsync(orderId, status, currentUserId, isAdmin: true, isSeller: false);

            if (result.Success)
            {
                TempData["Success"] = $"Order #{orderId} status updated to {status}.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Could not update order status.";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}
