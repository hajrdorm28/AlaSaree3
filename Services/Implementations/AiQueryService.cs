using Microsoft.EntityFrameworkCore;
using AlaSaree3.Data;
using AlaSaree3.DTOs.Ai;
using AlaSaree3.Models;
using AlaSaree3.Services.Interfaces;

namespace AlaSaree3.Services.Implementations
{
    public class AiQueryService : IAiQueryService
    {
        private readonly ApplicationDbContext _context;

        public AiQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductSearchResultDto> SearchProductsAsync(ProductSearchRequest request)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Reviews)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var term = request.Query.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Description.ToLower().Contains(term) ||
                    p.Category.Name.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                var cat = request.Category.Trim().ToLower();
                query = query.Where(p => p.Category.Name.ToLower().Contains(cat));
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= request.MaxPrice.Value);
            }

            query = request.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "rating" => query, // rating requires in-memory calc below; default order first
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            int totalItems = await query.CountAsync();
            int page = request.Page < 1 ? 1 : request.Page;
            int pageSize = request.PageSize is < 1 or > 24 ? 8 : request.PageSize;

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var summaries = products.Select(ToSummary).ToList();

            if (request.SortBy == "rating")
            {
                summaries = summaries.OrderByDescending(p => p.AverageRating).ToList();
            }

            return new ProductSearchResultDto
            {
                Products = summaries,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ProductDetailDto?> GetProductDetailAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Reviews)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return null;
            }

            var summary = ToSummary(product);
            return new ProductDetailDto
            {
                Id = summary.Id,
                Name = summary.Name,
                Price = summary.Price,
                AvailableQuantity = summary.AvailableQuantity,
                Category = summary.Category,
                SellerId = summary.SellerId,
                SellerName = summary.SellerName,
                AverageRating = summary.AverageRating,
                ReviewCount = summary.ReviewCount,
                AvailableSizes = summary.AvailableSizes,
                Url = summary.Url,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt
            };
        }

        public async Task<StockCheckDto?> CheckStockAsync(int productId, string? size)
        {
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                return null;
            }

            bool inStock = product.AvailableQuantity > 0;

            // If the product is size-based and a specific size was asked about, only report it
            // as available if that size is in the configured list AND overall stock is nonzero.
            if (inStock && !string.IsNullOrWhiteSpace(size) && !string.IsNullOrWhiteSpace(product.AvailableSizes))
            {
                var sizes = product.AvailableSizes.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                inStock = sizes.Any(s => s.Equals(size.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return new StockCheckDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                InStock = inStock,
                AvailableQuantity = product.AvailableQuantity,
                AvailableSizes = product.AvailableSizes
            };
        }

        public async Task<SellerInfoDto?> GetSellerInfoAsync(string sellerId)
        {
            var seller = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == sellerId);
            if (seller == null)
            {
                return null;
            }

            var sellerProducts = await _context.Products
                .Include(p => p.Reviews)
                .AsNoTracking()
                .Where(p => p.SellerId == sellerId)
                .ToListAsync();

            var allReviews = sellerProducts.SelectMany(p => p.Reviews).ToList();

            return new SellerInfoDto
            {
                SellerId = seller.Id,
                SellerName = seller.FullName,
                PhoneNumber = seller.PhoneNumber,
                JoinedAt = seller.CreatedAt,
                AverageRating = allReviews.Any() ? Math.Round(allReviews.Average(r => r.Rating), 1) : 0,
                ReviewCount = allReviews.Count,
                ActiveProductCount = sellerProducts.Count(p => p.AvailableQuantity > 0)
            };
        }

        public async Task<SellerInfoDto?> GetSellerInfoByProductAsync(int productId)
        {
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                return null;
            }

            return await GetSellerInfoAsync(product.SellerId);
        }

        public async Task<List<OrderSummaryDto>> GetCustomerOrdersAsync(string customerId, int? orderId, int take = 5)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Seller)
                .AsNoTracking()
                .Where(o => o.CustomerId == customerId);

            if (orderId.HasValue)
            {
                query = query.Where(o => o.Id == orderId.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Take(take)
                .ToListAsync();

            return orders.Select(o => new OrderSummaryDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                Items = o.Items.Select(i => new OrderItemSummaryDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "(deleted product)",
                    SellerName = i.Seller?.FullName ?? "(unknown seller)",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            }).ToList();
        }

        private static ProductSummaryDto ToSummary(Product p)
        {
            var reviews = p.Reviews ?? new List<Review>();
            return new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                AvailableQuantity = p.AvailableQuantity,
                Category = p.Category?.Name ?? string.Empty,
                SellerId = p.SellerId,
                SellerName = p.Seller?.FullName ?? string.Empty,
                AverageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0,
                ReviewCount = reviews.Count,
                AvailableSizes = p.AvailableSizes,
                Url = $"/Product/Details/{p.Id}"
            };
        }
    }
}
