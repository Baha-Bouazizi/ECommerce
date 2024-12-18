using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommerceElectronique.Data;
using CommerceElectronique.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CommerceElectronique.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Méthode pour vérifier si l'utilisateur est un administrateur
        private bool IsAdmin()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return !string.IsNullOrEmpty(userRole) && userRole == "Admin";
        }

        // Redirection si l'utilisateur n'est pas un administrateur
        private IActionResult RedirectIfNotAdmin()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Vous devez être administrateur pour accéder à cette section.";
                return RedirectToAction("Login", "Account");
            }

            return null;
        }

        public IActionResult Categories()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var categories = _context.Categories.ToList();
            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(Category category)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        public IActionResult EditCategory(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(Category category)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        public IActionResult DeleteCategory(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();
            return RedirectToAction(nameof(Categories));
        }

        public IActionResult Products()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var products = _context.Products.Include(p => p.Category).ToList();
            return View(products);
        }

        public IActionResult DetailsProduct(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var product = _context.Products
                                 .Include(p => p.Category)
                                 .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: /Admin/AddProduct
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile imageFile)
        {
            if (imageFile.Length > 0)
            {
                // Set the image path with the '/images/' prefix
                product.ImageUrl = "/images/" + Path.GetFileName(imageFile.FileName);

                var fileName = Path.GetFileName(imageFile.FileName);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                var filePath = Path.Combine(uploadsFolder, fileName);

                Directory.CreateDirectory(uploadsFolder);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, Product updatedProduct, IFormFile? imageFile)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Description = updatedProduct.Description;
            product.Stock = updatedProduct.Stock;
            product.CategoryId = updatedProduct.CategoryId;

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                product.ImageUrl = "/images/" + fileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            return product == null ? NotFound() : View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Products));
        }

        public IActionResult Dashboard()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            return View();
        }

        public IActionResult Orders()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var orders = _context.Orders
                                 .Include(o => o.User)
                                 .Where(o => o.Status == OrderStatus.Pending)
                                 .OrderByDescending(o => o.OrderDate)
                                 .ToList();

            return View(orders);
        }

        public async Task<IActionResult> Details(int orderId)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound();

            var orderViewModel = new OrderViewModel
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                UserName = order.User != null ? order.User.FirstName + " " + order.User.LastName : "Utilisateur inconnu",
                OrderDate = order.OrderDate,
                Status = order.Status,
                CartItems = order.CartItems.Select(ci => new CartItemViewModel
                {
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price,
                    Total = ci.Quantity * ci.Product.Price
                }).ToList(),
                TotalAmount = order.CartItems.Sum(ci => ci.Quantity * ci.Product.Price)
            };

            return View(orderViewModel);
        }
    }
}
