using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace CommerceElectronique.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        public string? ImageUrl { get; set; } = ""; // Utilisé pour stocker le chemin de l'image

     

        public int Stock { get; set; } = 1000;

        [ForeignKey("Category")] 
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int Popularity { get; set; } = 0;
        public DateTime DateAdded { get; set; }= DateTime.Now;

        public ICollection<CartItem> CartItems { get; set; }=new List<CartItem>();
        public ICollection<Order> OrderItems { get; set; } = new List<Order>();
    }
}
