using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CommerceElectronique.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [Required]
        public int Quantity { get; set; } // Quantité commandée.

        // Clé étrangère vers le produit
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Clé étrangère vers la commande
        [ForeignKey("Order")]
        public int? OrderId { get; set; } // Peut être null si pas encore commandé.
        public Order Order { get; set; }

        // Clé étrangère vers l'utilisateur (modifiée pour correspondre à IdentityUser)
        [ForeignKey("User")]
        public int   ? UserId { get; set; } // Utilisation de string pour correspondre à IdentityUser
        public User User { get; set; }
        public bool IsProcessed { get; set; } // Ajout de cette colonne

    }
}
