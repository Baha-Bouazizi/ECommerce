using FluentEmail.Core;
using FluentEmail.Smtp;
using System.Net;
using System.Net.Mail;
using CommerceElectronique.Data;
using CommerceElectronique.Helpers;
using CommerceElectronique.Interfaces;
using CommerceElectronique.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connexion � la base de donn�es
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configuration des param�tres Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Configuration des param�tres Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Enregistrement des services pour Cloudinary, Stripe, et email
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<StripeService>();
builder.Services.AddScoped<EmailService>();

// Configuration pour FluentEmail avec SMTP
var emailSettings = builder.Configuration.GetSection("EmailSettings");
builder.Services
    .AddFluentEmail(emailSettings["FromAddress"])  // Adresse de l'exp�diteur
    .AddSmtpSender(new SmtpClient(emailSettings["SmtpHost"])  // Serveur SMTP
    {
        Credentials = new NetworkCredential(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]),  // Identifiants SMTP
        EnableSsl = bool.Parse(emailSettings["SmtpEnableSsl"]),
        Port = int.Parse(emailSettings["SmtpPort"])  // Port pour SMTP avec SSL
    });

// Ajout des sessions avec m�moire cache
builder.Services.AddDistributedMemoryCache(); // Utilisation de la m�moire cache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Expiration apr�s 30 minutes d'inactivit�
    options.Cookie.HttpOnly = true; // Protection du cookie de session
    options.Cookie.IsEssential = true; // Rendre le cookie essentiel
});

// Ajout des services pour les contr�leurs et les vues
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurer le pipeline de traitement des requ�tes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Redirection vers HTTPS
app.UseHttpsRedirection();
app.UseStaticFiles(); // Ajout du middleware pour les fichiers statiques (CSS, JS, images)

// Middleware pour l'authentification et l'autorisation
app.UseAuthentication();
app.UseAuthorization();

// Middleware pour utiliser les sessions
app.UseSession();

// Middleware de routage
app.UseRouting();

// Appliquer les migrations et initialiser la base de donn�es
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Appliquer les migrations en attente
    context.Database.Migrate();
}

// Configurer la route par d�faut pour les contr�leurs et actions
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Undex}/{id?}");

app.Run();
