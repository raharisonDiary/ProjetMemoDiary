using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;
using SocialGasy.Models;
using System.Security.Claims;

namespace SocialGasy.Controllers
{
    public class AuthController : Controller
    {
        private readonly IMongoCollection<User> _users;
        private readonly ILogger<AuthController> _logger; // Nampiana Logger

        public AuthController(IMongoCollection<User> users, ILogger<AuthController> logger) 
        {
            _users = users;
            _logger = logger;
        }

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            _logger.LogInformation($"Miezaka miditra ny user: {username}");
            
            var user = await _users.Find(u => u.Username == username && u.Password == password).FirstOrDefaultAsync();
            
            if (user != null) {
                await SignInUser(user);
                _logger.LogInformation($"Tafiditra soa aman-tsara: {username}");
                return RedirectToAction("Index", "Home");
            }
            
            _logger.LogWarning($"Diso ny username na password ho an'ny: {username}");
            ViewBag.Error = "Username na Password diso.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginByScan(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) 
                return Json(new { success = false, message = "Code QR tsy manan-kery." });
            
            // Fadio ny espace be loatra
            var cleanToken = token.Trim();
            var user = await _users.Find(u => u.LoginToken == cleanToken).FirstOrDefaultAsync();
            
            if (user != null) {
                await SignInUser(user);
                _logger.LogInformation($"Tafiditra tamin'ny Scan: {user.Username}");
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            
            _logger.LogWarning($"Code tsy hita ao amin'ny DB: {cleanToken}");
            return Json(new { success = false, message = "Code tsy hita ao amin'ny rafitra." });
        }

        private async Task SignInUser(User user)
        {
            // ClaimTypes.Role dia tena zava-dehibe ho an'ny Authorization
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "User") // Raha null ny role dia "User"
            };
            
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            var authProperties = new AuthenticationProperties {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}