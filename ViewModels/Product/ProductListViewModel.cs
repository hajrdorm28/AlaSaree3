using AlaSaree3.Models;

namespace AlaSaree3.ViewModels.Product
{
    public class ProductListViewModel
    {
        public IEnumerable<AlaSaree3.Models.Product> Products { get; set; } = new List<AlaSaree3.Models.Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? SortBy { get; set; } // "price_asc", "price_desc", "newest", "name_asc"

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
