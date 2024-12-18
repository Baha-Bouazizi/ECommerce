namespace CommerceElectronique.Models
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public List<CartItemViewModel> CartItems { get; set; }
        public decimal TotalAmount { get; set; }

    }
}
