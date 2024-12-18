using CommerceElectronique.Data;
using CommerceElectronique.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceElectronique.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Afficher le panier
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == user.UserId)
                .ToListAsync();

            var total = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);
            ViewBag.Total = total;

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
            }

            return View(cartItems);
        }

        // Ajouter un produit au panier
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || quantity <= 0)
            {
                TempData["ErrorMessage"] = "Produit ou quantité invalide.";
                return RedirectToAction("Index", "Product");
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Vous devez être connecté pour ajouter un produit au panier.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (product.Stock < quantity)
            {
                TempData["ErrorMessage"] = "Stock insuffisant.";
                return RedirectToAction("Undex", "Product");
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.ProductId == productId && ci.UserId == user.UserId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = user.UserId
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                if (product.Stock < (cartItem.Quantity + quantity))
                {
                    TempData["ErrorMessage"] = "Stock insuffisant pour ajouter plus de ce produit.";
                    return RedirectToAction("Undex", "Product");
                }

                cartItem.Quantity += quantity;
            }

            product.Stock -= quantity;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Produit ajouté au panier.";
            return RedirectToAction("Undex", "Product");
        }

        // Supprimer un produit du panier
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (cartItem != null)
            {
                cartItem.Product.Stock += cartItem.Quantity;

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // Mettre à jour la quantité d'un produit dans le panier
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems.Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (cartItem != null && quantity > 0)
            {
                var product = cartItem.Product;

                int quantityDifference = quantity - cartItem.Quantity;

                if (product.Stock < quantityDifference)
                {
                    TempData["ErrorMessage"] = "Stock insuffisant pour mettre à jour la quantité.";
                    return RedirectToAction("Index");
                }

                product.Stock -= quantityDifference;

                cartItem.Quantity = quantity;

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
