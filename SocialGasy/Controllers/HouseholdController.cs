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
        public async Task<IActionResult> Create(Household household, string Latitude, string Longitude)
        {
            if (double.TryParse(Latitude?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
            {
                household.Latitude = lat;
            }

            if (double.TryParse(Longitude?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
            {
                household.Longitude = lon;
            }
            
            try 
            {
                household.CreatedAt = DateTime.UtcNow;
                household.CreatedByAgentId = User.Identity?.Name ?? "Système";

                await _householdCollection.InsertOneAsync(household);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERREUR DB : " + ex.Message);
                ModelState.AddModelError("", "Une erreur est survenue lors de l'enregistrement : " + ex.Message);
                return View(household);
            }
        }
    }
}