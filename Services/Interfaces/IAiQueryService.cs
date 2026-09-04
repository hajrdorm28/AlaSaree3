using AlaSaree3.DTOs.Ai;

namespace AlaSaree3.Services.Interfaces
{
    /// <summary>
    /// Read-side queries used exclusively by the AI shopping assistant's tool calls. Kept
    /// separate from the existing IProductService/IOrderService/etc. (which back the normal
    /// site UI) so the assistant's data shape can evolve independently without touching the
    /// core e-commerce flows.
    /// </summary>
    public interface IAiQueryService
    {
        Task<ProductSearchResultDto> SearchProductsAsync(ProductSearchRequest request);
        Task<ProductDetailDto?> GetProductDetailAsync(int productId);
        Task<StockCheckDto?> CheckStockAsync(int productId, string? size);
        Task<SellerInfoDto?> GetSellerInfoAsync(string sellerId);
        Task<SellerInfoDto?> GetSellerInfoByProductAsync(int productId);
        Task<List<OrderSummaryDto>> GetCustomerOrdersAsync(string customerId, int? orderId, int take = 5);
    }
}
