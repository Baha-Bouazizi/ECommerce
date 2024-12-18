namespace CommerceElectronique.Models
{
    public class CartItemViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; } // Calculé comme Quantity * Price
    }
}
