using Microsoft.AspNetCore.Mvc;
using AlaSaree3.DTOs.Ai;
using AlaSaree3.Services.Interfaces;

namespace AlaSaree3.Controllers.Api
{
    /// <summary>
    /// Internal, service-to-service API used only by the Python AI Assistant microservice to
    /// fulfil its tool calls (product search, policy lookup, order status, add-to-cart, etc).
    ///
    /// This is NOT meant to be called by the browser. It never uses the user's login cookie -
    /// the caller (the AI service) explicitly passes the acting customerId, and every request
    /// must present the shared X-Internal-Api-Key header configured in appsettings
    /// (AiService:InternalApiKey). The ASP.NET Core app and the AI service are expected to run
    /// on a private/trusted network (e.g. same docker-compose network); this header is a simple
    /// defense against the endpoint being hit directly from the public internet.
    /// </summary>
    [ApiController]
    [Route("api/internal/ai")]
    public class AiInternalController : ControllerBase
    {
        private readonly IAiQueryService _aiQueryService;
        private readonly IPolicyService _policyService;
        private readonly ICartService _cartService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiInternalController> _logger;

        public AiInternalController(
            IAiQueryService aiQueryService,
            IPolicyService policyService,
            ICartService cartService,
            IConfiguration configuration,
            ILogger<AiInternalController> logger)
        {
            _aiQueryService = aiQueryService;
            _policyService = policyService;
            _cartService = cartService;
            _configuration = configuration;
            _logger = logger;
        }

        [NonAction]
        private bool IsAuthorized()
        {
            var expectedKey = _configuration["AiService:InternalApiKey"];
            if (string.IsNullOrWhiteSpace(expectedKey))
            {
                // Fail closed: if no key is configured, refuse rather than leaving the endpoint open.
                _logger.LogWarning("AiService:InternalApiKey is not configured; refusing internal AI API request.");
                return false;
            }

            if (!Request.Headers.TryGetValue("X-Internal-Api-Key", out var provided))
            {
                return false;
            }

            return string.Equals(provided.ToString(), expectedKey, StringComparison.Ordinal);
        }

        [HttpGet("products/search")]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string? query,
            [FromQuery] string? category,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? sortBy,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 8)
        {
            if (!IsAuthorized()) return Unauthorized();

            var result = await _aiQueryService.SearchProductsAsync(new ProductSearchRequest
            {
                Query = query,
                Category = category,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                Page = page,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpGet("products/{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            if (!IsAuthorized()) return Unauthorized();

            var product = await _aiQueryService.GetProductDetailAsync(id);
            if (product == null) return NotFound(new { error = $"No product with id {id}." });

            return Ok(product);
        }

        [HttpGet("products/{id:int}/stock")]
        public async Task<IActionResult> CheckStock(int id, [FromQuery] string? size)
        {
            if (!IsAuthorized()) return Unauthorized();

            var stock = await _aiQueryService.CheckStockAsync(id, size);
            if (stock == null) return NotFound(new { error = $"No product with id {id}." });

            return Ok(stock);
        }

        [HttpGet("sellers/{id}")]
        public async Task<IActionResult> GetSeller(string id)
        {
            if (!IsAuthorized()) return Unauthorized();

            var seller = await _aiQueryService.GetSellerInfoAsync(id);
            if (seller == null) return NotFound(new { error = $"No seller with id {id}." });

            return Ok(seller);
        }

        [HttpGet("products/{id:int}/seller")]
        public async Task<IActionResult> GetSellerByProduct(int id)
        {
            if (!IsAuthorized()) return Unauthorized();

            var seller = await _aiQueryService.GetSellerInfoByProductAsync(id);
            if (seller == null) return NotFound(new { error = $"No seller found for product {id}." });

            return Ok(seller);
        }

        [HttpGet("sellers/{id}/policy")]
        public async Task<IActionResult> GetSellerPolicy(string id)
        {
            if (!IsAuthorized()) return Unauthorized();

            var bundle = await _policyService.GetSellerPolicyBundleAsync(id);
            if (bundle == null) return NotFound(new { error = $"No seller with id {id}." });

            return Ok(bundle);
        }

        [HttpGet("products/{id:int}/policy")]
        public async Task<IActionResult> GetSellerPolicyByProduct(int id)
        {
            if (!IsAuthorized()) return Unauthorized();

            var product = await _aiQueryService.GetProductDetailAsync(id);
            if (product == null) return NotFound(new { error = $"No product with id {id}." });

            var bundle = await _policyService.GetSellerPolicyBundleAsync(product.SellerId);
            if (bundle == null) return NotFound(new { error = $"No seller found for product {id}." });

            return Ok(bundle);
        }

        [HttpGet("platform/policies")]
        public async Task<IActionResult> GetAllPlatformPolicies()
        {
            if (!IsAuthorized()) return Unauthorized();

            var policies = await _policyService.GetAllPlatformPoliciesAsync();
            return Ok(policies);
        }

        [HttpGet("platform/policy/{key}")]
        public async Task<IActionResult> GetPlatformPolicy(string key)
        {
            if (!IsAuthorized()) return Unauthorized();

            var policy = await _policyService.GetPlatformPolicyAsync(key);
            if (policy == null) return NotFound(new { error = $"No platform policy with key '{key}'." });

            return Ok(policy);
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] string customerId, [FromQuery] int? orderId, [FromQuery] int take = 5)
        {
            if (!IsAuthorized()) return Unauthorized();

            if (string.IsNullOrWhiteSpace(customerId))
            {
                return BadRequest(new { error = "customerId is required." });
            }

            var orders = await _aiQueryService.GetCustomerOrdersAsync(customerId, orderId, take);
            return Ok(orders);
        }

        [HttpPost("cart/add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (!IsAuthorized()) return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.CustomerId) || request.ProductId <= 0 || request.Quantity <= 0)
            {
                return BadRequest(new AddToCartResultDto { Success = false, ErrorMessage = "customerId, productId and a positive quantity are required." });
            }

            var product = await _aiQueryService.GetProductDetailAsync(request.ProductId);

            var result = await _cartService.AddToCartAsync(request.CustomerId, request.ProductId, request.Quantity);

            return Ok(new AddToCartResultDto
            {
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
                ProductName = product?.Name
            });
        }
    }
}
