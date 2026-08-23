using AlaSaree3.Models;

namespace AlaSaree3.ViewModels.Order
{
    public class OrderDetailsViewModel
    {
        public AlaSaree3.Models.Order Order { get; set; } = null!;
        public IEnumerable<OrderItem> Items { get; set; } = new List<OrderItem>();
        public bool CanCancel => Order.Status == OrderStatus.Pending;
        public bool IsSellerView { get; set; }
        public bool IsAdminView { get; set; }
        public decimal SellerSubtotal => Items.Sum(i => i.UnitPrice * i.Quantity);
    }
}
