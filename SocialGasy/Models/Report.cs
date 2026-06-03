using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialGasy.Models
{
    public class Report
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty; // Nomena sanda default mba tsy ho null

        public string ChefName { get; set; } = string.Empty;
        public string ChefCin { get; set; } = string.Empty;
        public int TotalAgents { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Ohatra default
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } 
    }
}