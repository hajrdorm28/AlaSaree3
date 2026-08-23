using AlaSaree3.Models;
using AlaSaree3.ViewModels.Order;

namespace AlaSaree3.ViewModels.Seller
{
    public class SellerDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalOrdersCount { get; set; }
        public decimal TotalSalesRevenue { get; set; }

        public IEnumerable<AlaSaree3.Models.Product> RecentProducts { get; set; } = new List<AlaSaree3.Models.Product>();
        public List<SellerOrderItemDto> RecentOrders { get; set; } = new List<SellerOrderItemDto>();
    }
}
