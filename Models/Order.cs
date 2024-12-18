using CommerceElectronique.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace CommerceElectronique.Models
{
    public enum OrderStatus
    {
        Pending,
        Completed,
        Shipped,
        Cancelled
    }

    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public string ShippingAddress { get; set; }

        [Required]
        public string OrderNumber { get; set; }

        // Utilisation de l'enum pour définir l'état de la commande
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending; // "Pending" par défaut

        public ICollection<CartItem> CartItems { get; set; }

        public int  ? UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public decimal TotalAmount { get; set; } // Montant total ajouté
        public ICollection<OrderDetail> OrderDetails { get; set; }  // Assurez-vous que cette relation est correctement définie


    }
}