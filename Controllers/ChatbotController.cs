using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using AlaSaree3.Services.Interfaces;
using AlaSaree3.ViewModels.Chatbot;

namespace AlaSaree3.Controllers
{
    /// <summary>
    /// Browser-facing endpoint backing the AI shopping assistant widget. Available to both
    /// guests and logged-in users (guests get product/policy help; actions like "add to cart"
    /// or "order status" require login, which the assistant will explain if attempted while a
    /// guest).
    /// </summary>
    public class ChatbotController : Controller
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost]
        [Route("/Chatbot/Send")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send([FromBody] ChatSendRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.SessionKey))
            {
                return BadRequest(new ChatSendResponse
                {
                    Success = false,
                    Reply = string.Empty,
                    ErrorMessage = "A message and session key are required."
                });
            }

            var customerId = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var response = await _chatbotService.SendMessageAsync(request, customerId, User.Identity?.IsAuthenticated == true);
            return Ok(response);
        }

        [HttpGet]
        [Route("/Chatbot/History")]
        public async Task<IActionResult> History([FromQuery] string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                return Ok(new List<ChatHistoryItemViewModel>());
            }

            var customerId = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var history = await _chatbotService.GetHistoryAsync(sessionKey, customerId);
            return Ok(history);
        }
    }
}
