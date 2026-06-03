using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using SocialGasy.Models;
using SocialGasy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialGasy.Controllers
{
    [Authorize]
    public class CitizenController : Controller
    {
        private readonly IMongoCollection<Citizen> _citizenCollection;
        private readonly IMongoCollection<Household> _householdCollection;
        private readonly QRCodeService _qrCodeService;

        public CitizenController(IMongoDatabase database, QRCodeService qrCodeService)
        {
            _citizenCollection = database.GetCollection<Citizen>("Citizens");
            _householdCollection = database.GetCollection<Household>("Households");
            _qrCodeService = qrCodeService;
        }

        // Lisitry ny mponina rehetra
        public async Task<IActionResult> Index()
        {
            var citizens = await _citizenCollection.Find(_ => true).ToListAsync();
            var households = await _householdCollection.Find(_ => true).ToListAsync();
            ViewBag.Households = households.ToDictionary(h => h.Id ?? "", h => h);
            return View(citizens);
        }

        // Pejy fampidirana mponina vaovao
        public async Task<IActionResult> Create()
        {
            await RepopulateHouseholdsList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Citizen citizen)
        {
            if (string.IsNullOrEmpty(citizen.HouseholdId))
                ModelState.AddModelError("HouseholdId", "Mila misafidy Ménage iray ianao.");

            if (ModelState.IsValid)
            {
                var existing = await _citizenCollection.Find(c => c.CIN == citizen.CIN).FirstOrDefaultAsync();
                if (existing != null)
                {
                    ModelState.AddModelError("CIN", "Efa misy mampiasa io laharana CIN io.");
                    await RepopulateHouseholdsList();
                    return View(citizen);
                }
                
                citizen.QRCodeData = $"SOCIALGASY-{citizen.CIN}";
                await _citizenCollection.InsertOneAsync(citizen);
                
                // Fanitsiana: Mampiasa TempData hanamarika ny fahombiazana
                TempData["Success"] = "Voatahiry soa aman-tsara ny mponina!";
                
                // Mijanona eo amin'ny pejy Create mba hahafahana manohy ny fampidirana
                return RedirectToAction(nameof(Create));
            }
            
            await RepopulateHouseholdsList();
            return View(citizen);
        }

        // Pejy hijerena ny antsipiriany
        public async Task<IActionResult> Details(string id)
        {
            var citizen = await _citizenCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (citizen == null) return NotFound();

            var household = await _householdCollection.Find(h => h.Id == citizen.HouseholdId).FirstOrDefaultAsync();
            ViewBag.Household = household; 
            
            return View(citizen);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var citizen = await _citizenCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
            return citizen == null ? NotFound() : View(citizen);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _citizenCollection.DeleteOneAsync(c => c.Id == id);
            return RedirectToAction(nameof(Index));
        }

        private async Task RepopulateHouseholdsList()
        {
            var households = await _householdCollection.Find(_ => true).ToListAsync();
            ViewBag.Households = households.Select(h => new SelectListItem 
            { 
                Value = h.Id, 
                Text = $"{h.Fokontany} - {h.Address} ({h.District})" 
            }).ToList();
        }
    }
}