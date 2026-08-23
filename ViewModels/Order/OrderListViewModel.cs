using AlaSaree3.Models;

namespace AlaSaree3.ViewModels.Order
{
    public class OrderListViewModel
    {
        public IEnumerable<AlaSaree3.Models.Order> Orders { get; set; } = new List<AlaSaree3.Models.Order>();
    }
}
