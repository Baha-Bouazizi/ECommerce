using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CommerceElectronique.Models
{
    public class User 
    {
        [Key]
        public int   UserId { get; set; } // Clé primaire de l'utilisateur, cette propriété est en plus de l'Id généré par IdentityUser

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string ProfileImageUrl { get; set; } = "";// Optionnel : Image de profil.

        // Relation inverse vers les commandes passées par l'utilisateur
        public ICollection<Order> Orders{ get; set; }=new List<Order>();

        // Relation inverse vers les articles du panier
        public ICollection<CartItem> CartItems { get; set; }= new List<CartItem>();

        // Rôle de l'utilisateur (ex. Admin ou Utilisateur Simple)
        public string Role { get; set; } = "User"; // Add this field
    }
}
