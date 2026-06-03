using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SocialGasy.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;

namespace SocialGasy.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMongoCollection<User> _users;

        public AccountController(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Auth/Login.cshtml");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            var user = await _users.Find(u => u.Username != null && u.Username.ToLower() == username.ToLower() && u.Password == password).FirstOrDefaultAsync();

            if (user != null)
            {
                await SignInUser(user);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return user.Role switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin"),
                    "ChefRegional" => RedirectToAction("Dashboard", "Chef"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            ViewBag.Error = "Invalid username or password!";
            return View("~/Views/Auth/Login.cshtml");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LoginByScan(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Json(new { success = false, message = "Invalid token." });

            string cleanToken = token.Trim();
            var user = await _users.Find(u => u.LoginToken == cleanToken).FirstOrDefaultAsync();

            if (user != null)
            {
                await SignInUser(user);

                string redirectUrl = user.Role switch
                {
                    "Admin" => Url.Action("Dashboard", "Admin"),
                    "ChefRegional" => Url.Action("Dashboard", "Chef"),
                    _ => Url.Action("Index", "Home") 
                };

                return Json(new { success = true, redirectUrl = redirectUrl });
            }

            return Json(new { success = false, message = "Invalid QR Code." });
        }

        private async Task SignInUser(User user)
        {
            var role = !string.IsNullOrEmpty(user.Role) ? user.Role : "Agent";
            
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.FullName ?? "Utilisateur"),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) 
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        [Authorize]
        public async Task<IActionResult> Profil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId)) 
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _users.Find(u => u.Id.ToString() == userId).FirstOrDefaultAsync();
            
            if (user == null) 
            {
                return NotFound();
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}