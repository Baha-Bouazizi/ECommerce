using System.ComponentModel.DataAnnotations;

namespace CommerceElectronique.Models
{

    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; } // Description de la catégorie.

        // Relation inverse vers les produits de cette catégorie
        public ICollection<Product> Products { get; set; }=new List<Product>();
    }
}
