using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SocialGasy.Models;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace SocialGasy.Controllers
{
    [Authorize]
    public class HouseholdController : Controller
    {
        private readonly IMongoCollection<Household> _householdCollection;

        public HouseholdController(IMongoDatabase database)
        {
            _householdCollection = database.GetCollection<Household>("Households");
        }

        public async Task<IActionResult> Index()
        {
            var households = await _householdCollection.Find(_ => true).ToListAsync();
            return View(households);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Region,District,Commune,Fokontany,Address,ClientGuid")] Household household, string Latitude, string Longitude)
        {
            if (double.TryParse(Latitude?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
            {
                household.Latitude = lat;
            }

            if (double.TryParse(Longitude?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
            {
                household.Longitude = lon;
            }

            household.Id = null;
            household.CreatedAt = DateTime.UtcNow;
            household.CreatedByAgentId = User.Identity?.Name ?? "System";
            household.SyncStatus = "Synced";

            if (ModelState.IsValid)
            {
                try 
                {
                    await _householdCollection.InsertOneAsync(household);
                    TempData["Success"] = "Household saved successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving to database: " + ex.Message);
                }
            }
            
            return View(household);
        }

        public async Task<IActionResult> Details(string id)
        {
            var household = await _householdCollection.Find(h => h.Id == id).FirstOrDefaultAsync();
            if (household == null) return NotFound();

            return View(household);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var household = await _householdCollection.Find(h => h.Id == id).FirstOrDefaultAsync();
            return household == null ? NotFound() : View(household);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _householdCollection.DeleteOneAsync(h => h.Id == id);
            return RedirectToAction(nameof(Index));
        }
    }
}