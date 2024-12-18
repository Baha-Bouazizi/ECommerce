using CommerceElectronique.Data;
using CommerceElectronique.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceElectronique.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Index : Liste des produits avec pagination et filtres
        public async Task<IActionResult> Index(int page = 1, string search = "", string category = "", decimal? minPrice = null, decimal? maxPrice = null)
        {
            const int pageSize = 10;

            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // Filtrage
            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(search));
            }
            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Category.Name == category);
            }
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            // Calcul total pour pagination
            var totalProducts = await productsQuery.CountAsync();

            // Récupération des produits pour la page demandée
            var products = await productsQuery
                .OrderBy(p => p.Name) // Tri optionnel
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Préparation des données pour la vue
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Undex(int page = 1, string search = "", string category = "", decimal? minPrice = null, decimal? maxPrice = null)
        {
            const int pageSize = 20;

            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // Filtrage
            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(search));
            }
            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Category.Name == category);
            }
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            // Calcul total pour pagination
            var totalProducts = await productsQuery.CountAsync();

            // Récupération des produits pour la page demandée
            var products = await productsQuery
                .OrderBy(p => p.Name) // Tri optionnel
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Préparation des données pour la vue
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(products);
        }

        // Details : Détails d'un produit spécifique
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    
        


    }
}
