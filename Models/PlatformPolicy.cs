using System.ComponentModel.DataAnnotations;

namespace AlaSaree3.Models
{
    /// <summary>
    /// Site-wide (platform-level) policies that apply to every seller/order unless a seller
    /// overrides them with their own SellerPolicy. Used by the AI shopping assistant to answer
    /// general questions such as "What is the website's return policy?".
    /// </summary>
    public class PlatformPolicy
    {
        public int Id { get; set; }

        /// <summary>
        /// Stable machine-readable key, e.g. "Return", "Shipping", "Cancellation", "Payment", "Warranty".
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Content { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
