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

        public async Task<IActionResult> Index()
        {
            var citizens = await _citizenCollection.Find(_ => true).ToListAsync();
            var households = await _householdCollection.Find(_ => true).ToListAsync();
            ViewBag.Households = households.ToDictionary(h => h.Id ?? "", h => h);
            return View(citizens);
        }

        public async Task<IActionResult> Create()
        {
            await RepopulateHouseholdsList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Citizen citizen)
        {
            ModelState.Remove("Id");
            ModelState.Remove("RegisteredAt");
            ModelState.Remove("SpouseName");
            ModelState.Remove("NumberOfChildren");
            ModelState.Remove("Profession");

            if (string.IsNullOrEmpty(citizen.HouseholdId))
            {
                ModelState.AddModelError("HouseholdId", "Selection required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _citizenCollection.Find(c => c.CIN == citizen.CIN).FirstOrDefaultAsync();
                    if (existing != null)
                    {
                        ModelState.AddModelError("CIN", "This CIN is already in use.");
                        await RepopulateHouseholdsList();
                        return View(citizen);
                    }

                    citizen.Id = null;
                    citizen.QRCodeData = $"SOCIALGASY-{citizen.CIN}";
                    citizen.RegisteredAt = DateTime.UtcNow;
                    citizen.SyncStatus = "Synced";

                    await _citizenCollection.InsertOneAsync(citizen);

                    TempData["Success"] = "Citizen saved successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Database error: " + ex.Message);
                }
            }

            await RepopulateHouseholdsList();
            return View(citizen);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var citizen = await _citizenCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (citizen == null) return NotFound();

            await RepopulateHouseholdsList();
            return View(citizen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Citizen citizen)
        {
            if (id != citizen.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var filter = Builders<Citizen>.Filter.Eq(c => c.Id, id);
                    var update = Builders<Citizen>.Update
                        .Set(c => c.CIN, citizen.CIN)
                        .Set(c => c.LastName, citizen.LastName)
                        .Set(c => c.FirstName, citizen.FirstName)
                        .Set(c => c.DateOfBirth, citizen.DateOfBirth)
                        .Set(c => c.Gender, citizen.Gender)
                        .Set(c => c.MaritalStatus, citizen.MaritalStatus)
                        .Set(c => c.NumberOfChildren, citizen.NumberOfChildren)
                        .Set(c => c.Profession, citizen.Profession)
                        .Set(c => c.HouseholdId, citizen.HouseholdId)
                        .Set(c => c.SyncStatus, "Synced");

                    var result = await _citizenCollection.UpdateOneAsync(filter, update);

                    if (result.MatchedCount > 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("", "Update failed.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }
            await RepopulateHouseholdsList();
            return View(citizen);
        }

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