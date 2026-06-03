using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using SocialGasy.Models;
using System.Threading.Tasks;
using System;

namespace SocialGasy.Controllers
{
    [Authorize(Roles = "ChefRegional")]
    public class ChefController : Controller
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Report> _reports;

        public ChefController(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
            _reports = database.GetCollection<Report>("Reports");
        }

        // Action Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var agents = await _users.Find(u => u.Role == "Agent").ToListAsync();
            return View(agents);
        }

        // Action SendReport
        public IActionResult SendReport()
        {
            return View();
        }

        // 1. Action ho an'ny Details
public async Task<IActionResult> Details(string id)
{
    if (string.IsNullOrEmpty(id)) return NotFound();

    // Mitady ny Agent amin'ny alalan'ny ID
    var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    
    if (user == null) return NotFound();

    return View(user);
}

// 2. Action ho an'ny Edit (GET - mampiseho ny form)
public async Task<IActionResult> Edit(string id)
{
    if (string.IsNullOrEmpty(id)) return NotFound();

    var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    
    if (user == null) return NotFound();

    return View(user);
}

// 3. Action ho an'ny Edit (POST - mitahiry ny fanovana)
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(string id, User user)
{
    if (id != user.Id) return NotFound();

    if (ModelState.IsValid)
    {
        // Fanavaozana ny data ao amin'ny MongoDB
        await _users.ReplaceOneAsync(u => u.Id == id, user);
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
                // Kajy ny isan'ny agent
                var totalAgents = await _users.CountDocumentsAsync(u => u.Role == "Agent");
                
                // Fenoy ireo saha ilaina
                report.TotalAgents = (int)totalAgents;
                report.CreatedAt = DateTime.UtcNow;
                report.Status = "Pending";

                await _reports.InsertOneAsync(report);
                
                // Mampiasa nameof() mba ho azo antoka ny redirection
                return RedirectToAction(nameof(Dashboard)); 
            }
            return View(report);
        }
    }
}