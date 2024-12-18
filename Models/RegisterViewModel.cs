using System.ComponentModel.DataAnnotations;

namespace CommerceElectronique.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Le prénom est requis.")]
        [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est requis.")]
        [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [StringLength(100, ErrorMessage = "Le mot de passe doit comporter au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est requise.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Le mot de passe et sa confirmation ne correspondent pas.")]
        public string ConfirmPassword { get; set; }

        public string ProfileImageUrl { get; set; } // Optionnel
    }
}
