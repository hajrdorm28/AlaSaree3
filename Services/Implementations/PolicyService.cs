using Microsoft.EntityFrameworkCore;
using AlaSaree3.Data;
using AlaSaree3.DTOs.Ai;
using AlaSaree3.Services.Interfaces;

namespace AlaSaree3.Services.Implementations
{
    public class PolicyService : IPolicyService
    {
        private readonly ApplicationDbContext _context;

        public PolicyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PolicyDto?> GetPlatformPolicyAsync(string key)
        {
            var policy = await _context.PlatformPolicies
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Key.ToLower() == key.ToLower());

            if (policy == null)
            {
                return null;
            }

            return new PolicyDto
            {
                Key = policy.Key,
                Title = policy.Title,
                Content = policy.Content,
                Source = "Platform"
            };
        }

        public async Task<List<PolicyDto>> GetAllPlatformPoliciesAsync()
        {
            var policies = await _context.PlatformPolicies.AsNoTracking().ToListAsync();
            return policies.Select(p => new PolicyDto
            {
                Key = p.Key,
                Title = p.Title,
                Content = p.Content,
                Source = "Platform"
            }).ToList();
        }

        public async Task<SellerPolicyBundleDto?> GetSellerPolicyBundleAsync(string sellerId)
        {
            var seller = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == sellerId);
            if (seller == null)
            {
                return null;
            }

            var sellerPolicy = await _context.SellerPolicies
                .AsNoTracking()
                .FirstOrDefaultAsync(sp => sp.SellerId == sellerId);

            var platformReturn = await GetPlatformPolicyAsync("Return");
            var platformShipping = await GetPlatformPolicyAsync("Shipping");
            var platformWarranty = await GetPlatformPolicyAsync("Warranty");

            return new SellerPolicyBundleDto
            {
                SellerId = seller.Id,
                SellerName = seller.FullName,
                Return = ResolvePolicy("Return", "Return Policy", sellerPolicy?.ReturnPolicy, platformReturn),
                Shipping = ResolvePolicy("Shipping", "Shipping Policy", sellerPolicy?.ShippingPolicy, platformShipping),
                Warranty = ResolvePolicy("Warranty", "Warranty Policy", sellerPolicy?.WarrantyPolicy, platformWarranty)
            };
        }

        /// <summary>
        /// A seller's own text wins if they've set it; otherwise fall back to the platform policy.
        /// If neither exists, returns an explicit "not available" marker rather than guessing.
        /// </summary>
        private static PolicyDto ResolvePolicy(string key, string title, string? sellerText, PolicyDto? platformFallback)
        {
            if (!string.IsNullOrWhiteSpace(sellerText))
            {
                return new PolicyDto { Key = key, Title = title, Content = sellerText, Source = "Seller" };
            }

            if (platformFallback != null)
            {
                return platformFallback;
            }

            return new PolicyDto
            {
                Key = key,
                Title = title,
                Content = "No specific policy is on file for this yet.",
                Source = "None"
            };
        }
    }
}
