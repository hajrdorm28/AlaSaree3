using System.ComponentModel.DataAnnotations;

namespace AlaSaree3.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;
        public virtual ApplicationUser Customer { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
    }
}
