using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using SocialGasy.Models;
using System.Threading.Tasks;
using System;
using QRCoder; // Aza adino ny install QRCoder
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace SocialGasy.Controllers
{
    [Authorize(Roles = "ChefRegional")]
    public class ChefController : Controller
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Report> _reports;
        private static readonly HttpClient _httpClient = new HttpClient();

        public ChefController(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
            _reports = database.GetCollection<Report>("Reports");
        }

        public async Task<IActionResult> Dashboard()
        {
            var agents = await _users.Find(u => u.Role == "Agent").ToListAsync();
            return View(agents);
        }

        // --- Ampiana ny fiasa famoronana Agent ---
        public IActionResult CreateAgent() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAgent(User user)
        {
            if (ModelState.IsValid)
            {
                user.Role = "Agent";
                user.CreatedAt = DateTime.UtcNow;
                user.LoginToken = Guid.NewGuid().ToString(); 
                
                // Generer QR Code
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(user.LoginToken, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                user.QrCodeImage = qrCodeImage;

                await _users.InsertOneAsync(user);

                // Mandefa hafatra amin'ny WhatsApp
                string base64Image = Convert.ToBase64String(qrCodeImage);
                string apiUrl = "https://api.ultramsg.com/instance179269/messages/image";
                
                var payload = new
                {
                    token = "z8bor2yc7ladu21d",
                    to = user.PhoneNumber,
                    image = $"data:image/png;base64,{base64Image}",
                    caption = "Bonjour, voici votre code QR pour accéder à votre espace Agent."
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync(apiUrl, content);

                TempData["Success"] = "Agent créé avec succès et code QR envoyé !";
                return RedirectToAction(nameof(Dashboard));
            }
            return View(user);
        }

        public IActionResult SendReport() => View();

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return NotFound();
            return View(user);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User user)
        {
            if (id != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _users.ReplaceOneAsync(u => u.Id == id, user);
                TempData["Success"] = "Modifications enregistrées.";
                return RedirectToAction(nameof(Dashboard));
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendReport(Report report)
        {
            if (ModelState.IsValid)
            {
                var totalAgents = await _users.CountDocumentsAsync(u => u.Role == "Agent");
                report.TotalAgents = (int)totalAgents;
                report.CreatedAt = DateTime.UtcNow;
                report.Status = "Pending";

                await _reports.InsertOneAsync(report);
                TempData["Success"] = "Rapport envoyé avec succès.";
                return RedirectToAction(nameof(Dashboard)); 
            }
            return View(report);
        }
    }
}