using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SocialGasy.Models;
using System.Threading.Tasks;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SocialGasy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Report> _reports;
        private static readonly HttpClient _httpClient = new HttpClient();

        public AdminController(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
            _reports = database.GetCollection<Report>("Reports");
        }

        public async Task<IActionResult> Dashboard()
        {
            var allReports = await _reports.Find(_ => true) 
                                           .SortByDescending(r => r.CreatedAt)
                                           .ToListAsync();
            return View(allReports);
        }

        public async Task<IActionResult> ListChefs()
        {
            var chefs = await _users.Find(u => u.Role == "ChefRegional").ToListAsync();
            return View(chefs);
        }

        public IActionResult CreateChef() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChef(User user)
        {
            if (ModelState.IsValid)
            {
                user.Role = "ChefRegional";
                user.CreatedAt = DateTime.UtcNow;
                user.LoginToken = Guid.NewGuid().ToString(); 
                
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(user.LoginToken, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                
                user.QrCodeImage = qrCodeImage;

                await _users.InsertOneAsync(user);

                string base64Image = Convert.ToBase64String(qrCodeImage);
                string apiUrl = "https://api.ultramsg.com/instance179269/messages/image";
                
                var payload = new
                {
                    token = "z8bor2yc7ladu21d",
                    to = user.PhoneNumber,
                    image = $"data:image/png;base64,{base64Image}",
                    caption = "Voici votre code QR de connexion."
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync(apiUrl, content);

                TempData["Success"] = "Chef créé avec succès !";
                return RedirectToAction(nameof(ListChefs));
            }
            return View(user);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return NotFound();
            return View("EditChef", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update
                .Set(u => u.FullName, user.FullName)
                .Set(u => u.Email, user.Email)
                .Set(u => u.PhoneNumber, user.PhoneNumber)
                .Set(u => u.Address, user.Address);

            await _users.UpdateOneAsync(filter, update);
            TempData["Success"] = "Modifications enregistrées.";
            return RedirectToAction(nameof(ListChefs));
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return NotFound();
            return View("Delete", user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _users.DeleteOneAsync(u => u.Id == id);
            TempData["Success"] = "Chef supprimé.";
            return RedirectToAction(nameof(ListChefs));
        }

        public async Task<IActionResult> Notifications()
        {
            var reports = await _reports.Find(_ => true)
                                        .SortByDescending(r => r.CreatedAt)
                                        .ToListAsync();
            return View(reports);
        }

        public async Task<IActionResult> JereoRapport(string id)
        {
            var report = await _reports.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (report == null) return NotFound();

            if (!report.IsRead)
            {
                var filter = Builders<Report>.Filter.Eq(r => r.Id, id);
                var update = Builders<Report>.Update.Set(r => r.IsRead, true);
                
                await _reports.UpdateOneAsync(filter, update);
            }

            return View(report);
        }
    }
}