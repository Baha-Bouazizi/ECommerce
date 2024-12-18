using CommerceElectronique.Data;
using CommerceElectronique.Models;
using CommerceElectronique.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceElectronique.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Page d'inscription
        public IActionResult Register()
        {
            return View();
        }

        // Inscription d'un utilisateur
        [HttpPost]
        public async Task<IActionResult> Register(User user, string password)
        {
            if (ModelState.IsValid)
            {
                // Vérification si l'email existe déjà
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Un utilisateur avec cet email existe déjà.");
                    return View(user);
                }

                // Hashage du mot de passe
                user.Password = BCrypt.Net.BCrypt.HashPassword(password);

                // Attribution du rôle
                user.Role = user.Email == "baha.bouazizi@isimg.tn" ? "Admin" : "User";

                // Ajout de l'utilisateur à la base de données
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Inscription réussie. Vous pouvez maintenant vous connecter.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // Page de connexion
        public IActionResult Login()
        {
            return View();
        }

        // Connexion d'un utilisateur
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
                return View();
            }

            // Authentification réussie
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserRole", user.Role);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return RedirectToAction("Undex", "Product");
            }
        }

        // Déconnexion de l'utilisateur
        [HttpPost]
        public IActionResult Logout()
        {
            // Effacer les informations de l'utilisateur de la session
            HttpContext.Session.Clear();

            // Rediriger vers la page de connexion ou d'accueil
            return RedirectToAction("Login");
        }
        // Action pour afficher le profil de l'utilisateur connecté
     public async Task<IActionResult> Profile()
{
    var userId = HttpContext.Session.GetString("UserId");

    if (userId == null)
    {
        return RedirectToAction("Login");
    }

    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

    if (user == null)
    {
        return RedirectToAction("Login");
    }

    // Retrieve order history for the user
    var orderHistory = await _context.Orders
        .Where(o => o.UserId == Convert.ToInt32(userId))
        .Include(o => o.CartItems)
        .ThenInclude(ci => ci.Product)
        .ToListAsync();

    // Add order history to the user model before passing to the view
    var userProfileModel = new UserProfileViewModel
    {
        User = user,
        OrderHistorys = orderHistory
    };

    return View(userProfileModel);
}


        // Action pour afficher le formulaire d'édition du profil
        // Action pour afficher le formulaire d'édition du profil
        // Action pour afficher le formulaire d'édition du profil
        public async Task<IActionResult> Edit()
        {
            // Récupérer l'ID de l'utilisateur connecté depuis la session
            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            // Récupérer les informations de l'utilisateur
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user); // Passer l'utilisateur à la vue pour l'édition
        }



        // Action pour enregistrer les modifications du profil
        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            if (ModelState.IsValid)
            {
                // Vérifier si l'ID de l'utilisateur dans la session correspond à celui du formulaire
                var sessionUserId = HttpContext.Session.GetString("UserId");

                if (sessionUserId == null || sessionUserId != user.UserId.ToString())
                {
                    // L'utilisateur n'est pas connecté ou il tente de modifier un autre profil
                    return RedirectToAction("Login");
                }

                // Récupérer l'utilisateur dans la base de données pour le mettre à jour
                var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);

                if (dbUser == null)
                {
                    // Si l'utilisateur n'est pas trouvé dans la base de données
                    return RedirectToAction("Login");
                }

                // Mettre à jour les informations de l'utilisateur sauf l'email
                dbUser.FirstName = user.FirstName;
                dbUser.LastName = user.LastName;

                // Si le mot de passe a été modifié, le re-hasher
                if (!string.IsNullOrEmpty(user.Password))
                {
                    dbUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                // Sauvegarder les modifications dans la base de données
                _context.Users.Update(dbUser);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Votre profil a été mis à jour avec succès.";
                return RedirectToAction("Profile");
            }

            // En cas d'erreur, renvoyer l'utilisateur à la vue d'édition
            return View(user);
        }





    }

}
