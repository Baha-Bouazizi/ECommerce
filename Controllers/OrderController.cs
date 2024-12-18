using CommerceElectronique.Data;
using CommerceElectronique.Models;
using CommerceElectronique.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly StripeSettings _stripeSettings;
    private readonly EmailService _emailService;
    private readonly IConfiguration _configuration;

    public OrderController(ApplicationDbContext context, IOptions<StripeSettings> stripeSettings, EmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        _emailService = emailService;
        _configuration = configuration;
    }

    // Affichage du formulaire Checkout
    public IActionResult Checkout()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "You must be logged in to checkout.";
            return RedirectToAction("Login", "Account");
        }

        // Récupérer les articles du panier de l'utilisateur connecté
        var cartItems = _context.CartItems
            .Where(ci => ci.UserId == Convert.ToInt32(userId))
            .Include(ci => ci.Product)
            .ToList();

        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index", "Home");
        }

        var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);
        ViewBag.TotalAmount = totalAmount;

        return View(cartItems);
    }

    // Création d'une commande et gestion du paiement via Stripe
    [HttpPost]
    public async Task<IActionResult> CreateOrder(string shippingAddress, string stripeToken)
    {
        var userId = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "You must be logged in to create an order.";
            return RedirectToAction("Login", "Account");
        }

        var cartItems = _context.CartItems
            .Where(ci => ci.UserId == Convert.ToInt32(userId))
            .Include(ci => ci.Product)
            .ToList();

        var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);

        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Checkout");
        }

        var order = new Order
        {
            OrderDate = DateTime.Now,
            ShippingAddress = shippingAddress,
            OrderNumber = Guid.NewGuid().ToString(),
            CartItems = cartItems,
            Status = OrderStatus.Pending,
            UserId = Convert.ToInt32(userId)
        };

        foreach (var cartItem in cartItems)
        {
            cartItem.OrderId = order.OrderId;
            var product = cartItem.Product;
            if (product != null)
            {
                product.Popularity += cartItem.Quantity;
            }
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Traitement du paiement avec Stripe
        var options = new ChargeCreateOptions
        {
            Amount = (long)(totalAmount * 100),
            Currency = "usd",
            Description = $"Order #{order.OrderNumber}",
            Source = stripeToken,
            Metadata = new Dictionary<string, string> { { "orderId", order.OrderId.ToString() } }
        };

        var service = new ChargeService();
        var charge = await service.CreateAsync(options);

        if (charge.Status == "succeeded")
        {
            TempData["SuccessMessage"] = "Your order has been placed successfully!";
            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
        }
        else
        {
            TempData["ErrorMessage"] = "Payment failed. Please try again.";
            return RedirectToAction("Checkout");
        }
    }


    // Affichage de la confirmation de commande
    public async Task<IActionResult> OrderConfirmation(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            return NotFound();
        }

        var totalAmount = order.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
        ViewBag.TotalAmount = totalAmount;

        ViewBag.OrderItems = order.CartItems.Select(ci => new
        {
            ProductName = ci.Product.Name,
            Quantity = ci.Quantity,
            Price = ci.Product.Price,
            Subtotal = ci.Quantity * ci.Product.Price
        }).ToList();

        // Marquer les éléments comme traités au lieu de les supprimer
        foreach (var cartItem in order.CartItems)
        {
            cartItem.IsProcessed = true;
        }

        _context.UpdateRange(order.CartItems); // Mettre à jour les CartItems
        await _context.SaveChangesAsync();

        return View(order);
    }


    // Afficher l'historique des commandes
    // Afficher l'historique des commandes
    public async Task<IActionResult> OrderHistory()
    {
        var userId = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Vous devez être connecté pour consulter votre historique de commandes.";
            return RedirectToAction("Login", "Account");
        }

        var orders = await _context.Orders
            .Where(o => o.UserId == Convert.ToInt32(userId))
            .Include(o => o.CartItems)
            .ThenInclude(ci => ci.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        var orderHistoryViewModel = orders.Select(order => new
        {
            OrderId = order.OrderId,
            OrderDate = order.OrderDate,
            ShippingAddress = order.ShippingAddress,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            Items = order.CartItems.Select(ci => new
            {
                ProductName = ci.Product.Name,
                Quantity = ci.Quantity,
                Price = ci.Product.Price,
                Subtotal = ci.Quantity * ci.Product.Price
            })
        });

        return View(orderHistoryViewModel);
    }



    // Méthode pour accepter une commande (Admin)
    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> ApproveOrder(int orderId)
    {
        var order = await _context.Orders
                                  .Include(o => o.User)
                                  .Include(o => o.CartItems)
                                  .ThenInclude(ci => ci.Product) // Inclure les produits
                                  .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction("Orders", "Admin");
        }

        if (order.Status != OrderStatus.Pending)
        {
            TempData["ErrorMessage"] = "This order has already been processed.";
            return RedirectToAction("Orders", "Admin");
        }

        // Calculer le montant total de la commande
        var totalAmount = order.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

        // Créer une liste des produits dans la commande
        var orderItems = order.CartItems.Select(ci => new
        {
            ProductName = ci.Product.Name,
            Quantity = ci.Quantity,
            Price = ci.Product.Price,
            Subtotal = ci.Quantity * ci.Product.Price
        }).ToList();

        // Marquer la commande comme complétée
        order.Status = OrderStatus.Completed;

        // Mise à jour de l'ordre avec le nouveau TotalAmount
        order.TotalAmount = totalAmount;

        _context.Update(order);  // Mise à jour de l'ordre
        await _context.SaveChangesAsync();

        // Vider le panier de l'utilisateur après la commande
        if (order.User != null)
        {
            var userId = order.User.UserId;

            // Supprimer tous les éléments du panier pour cet utilisateur
            var cartItems = await _context.CartItems
                                          .Where(ci => ci.UserId == userId)
                                          .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        // Envoyer un email à l'utilisateur pour lui notifier de l'acceptation de la commande
        if (order.User != null)
        {
            var email = order.User.Email;
            var subject = "Order Accepted";
            var body = $@"
        <html>
            <body>
                <h2>Order Confirmation</h2>
                <p>Dear {order.User.LastName},</p>
                <p>Your order #{order.OrderNumber} has been accepted and is now being processed.</p>
                <h3>Order Details:</h3>
                <table border='1' cellpadding='5'>
                    <thead>
                        <tr>
                            <th>Product Name</th>
                            <th>Quantity</th>
                            <th>Price</th>
                            <th>Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>";

            foreach (var item in orderItems)
            {
                body += $@"
                <tr>
                    <td>{item.ProductName}</td>
                    <td>{item.Quantity}</td>
                    <td>{item.Price.ToString("C", new CultureInfo("fr-FR"))}</td>
                    <td>{(item.Quantity * item.Price).ToString("C", new CultureInfo("fr-FR"))}</td>
                </tr>";
            }

            body += $@"
            </tbody>
        </table>
        <h3>Total Amount: {totalAmount.ToString("C", new CultureInfo("fr-FR"))}</h3>
        <p>Thank you for shopping with us!</p>
    </body>
</html>";

            var fromAddress = _configuration["EmailSettings:FromAddress"];
            var fromName = _configuration["EmailSettings:FromName"];
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var smtpEnableSsl = bool.Parse(_configuration["EmailSettings:SmtpEnableSsl"]);

            var fromMailAddress = new MailAddress(fromAddress, fromName);
            var toMailAddress = new MailAddress(email);

            using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
            {
                var mailMessage = new MailMessage(fromMailAddress, toMailAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = smtpEnableSsl;

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                    TempData["SuccessMessage"] = "Order has been accepted and confirmation email sent.";
                }
                catch (SmtpException smtpEx)
                {
                    TempData["ErrorMessage"] = $"SMTP error: {smtpEx.Message}";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Failed to send email: {ex.Message}";
                }
            }
        }

        return RedirectToAction("Orders", "Admin");
    }









}
