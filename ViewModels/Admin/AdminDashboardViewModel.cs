using AlaSaree3.Models;

namespace AlaSaree3.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalSellers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PendingSellerRequests { get; set; }
        public decimal TotalRevenue { get; set; }

        public IEnumerable<AlaSaree3.Models.Order> RecentOrders { get; set; } = new List<AlaSaree3.Models.Order>();
        public IEnumerable<SellerRequest> RecentSellerRequests { get; set; } = new List<SellerRequest>();
    }
}
