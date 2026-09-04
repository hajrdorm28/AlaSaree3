using AlaSaree3.DTOs.Ai;

namespace AlaSaree3.Services.Interfaces
{
    /// <summary>
    /// Resolves policy questions ("return policy", "shipping policy", etc.) with the correct
    /// precedence: a seller's own SellerPolicy field wins when set, otherwise the platform-wide
    /// PlatformPolicy is used as a fallback. This is the single place that owns the
    /// seller-vs-platform distinction so the AI assistant can never accidentally mix them up.
    /// </summary>
    public interface IPolicyService
    {
        Task<PolicyDto?> GetPlatformPolicyAsync(string key);
        Task<List<PolicyDto>> GetAllPlatformPoliciesAsync();
        Task<SellerPolicyBundleDto?> GetSellerPolicyBundleAsync(string sellerId);
    }
}
