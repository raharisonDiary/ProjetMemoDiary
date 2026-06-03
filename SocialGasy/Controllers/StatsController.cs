using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialGasy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialGasy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatsController : Controller
    {
        private readonly IMongoCollection<Citizen> _citizens;
        private readonly IMongoCollection<Household> _households;

        public StatsController(IMongoDatabase database)
        {
            _citizens = database.GetCollection<Citizen>("Citizens");
            _households = database.GetCollection<Household>("Households");
        }
        public async Task<IActionResult> Index(string? district = null)
{
    var districts = await _households.Distinct(h => h.District, FilterDefinition<Household>.Empty).ToListAsync();
    ViewBag.Districts = districts;
    ViewBag.SelectedDistrict = district;

    var pipeline = new BsonDocument[]
    {
        new BsonDocument("$lookup", new BsonDocument {
            { "from", "Households" },
            { "localField", "HouseholdId" },
            { "foreignField", "_id" },
            { "as", "HouseholdDetails" }
        }),
        new BsonDocument("$unwind", "$HouseholdDetails")
    };

    var finalPipeline = pipeline.ToList();
    if (!string.IsNullOrEmpty(district))
    {
        finalPipeline.Add(new BsonDocument("$match", new BsonDocument("HouseholdDetails.District", district)));
    }

    var cursor = await _citizens.AggregateAsync<BsonDocument>(finalPipeline);
    var results = await cursor.ToListAsync();

    var finalStats = results.Select(c => {
        var dob = c.Contains("DateOfBirth") ? c["DateOfBirth"].ToUniversalTime() : DateTime.UtcNow;
        int age = DateTime.UtcNow.Year - dob.Year;
        string group = age < 18 ? "Mineur (<18)" : age <= 35 ? "Jeune (18-35)" : age <= 60 ? "Adulte (36-60)" : "Senior (>60)";
        return new { Gender = c.Contains("Gender") ? c["Gender"].AsString : "Unknown", AgeGroup = group };
    })
    .GroupBy(x => new { x.AgeGroup, x.Gender })
    .Select(g => new { g.Key.AgeGroup, g.Key.Gender, Count = g.Count() }).ToList();

    var filter = string.IsNullOrEmpty(district) ? FilterDefinition<Household>.Empty : Builders<Household>.Filter.Eq(h => h.District, district);
    
    ViewBag.TotalCitizens = results.Count; 
    
    ViewBag.TotalHouseholds = await _households.CountDocumentsAsync(filter);

    var topPipeline = new BsonDocument[]
    {
        new BsonDocument("$lookup", new BsonDocument {
            { "from", "Households" },
            { "localField", "HouseholdId" },
            { "foreignField", "_id" },
            { "as", "h" }
        }),
        new BsonDocument("$unwind", "$h"),
        new BsonDocument("$group", new BsonDocument {
            { "_id", "$h.District" },
            { "Count", new BsonDocument("$sum", 1) }
        }),
        new BsonDocument("$sort", new BsonDocument("Count", -1)),
        new BsonDocument("$limit", 5)
    };
    
    var topCursor = await _citizens.AggregateAsync<BsonDocument>(topPipeline);
    ViewBag.TopDistricts = await topCursor.ToListAsync();

    ViewBag.Stats = finalStats;
    return View();
}
    }
}