using AlaSaree3.Models;

namespace AlaSaree3.ViewModels.Product
{
    public class ProductDetailsViewModel
    {
        public AlaSaree3.Models.Product Product { get; set; } = null!;
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public IEnumerable<AlaSaree3.Models.Review> Reviews { get; set; } = new List<AlaSaree3.Models.Review>();
        public Dictionary<int, int> RatingCounts { get; set; } = new Dictionary<int, int>();

        public bool CanReview { get; set; }
        public bool HasReviewed { get; set; }
        public bool IsInWishlist { get; set; }
        public bool IsOwner { get; set; }
    }
}
