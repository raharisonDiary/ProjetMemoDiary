using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace SocialGasy.Models
{
    public class Household
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? ClientGuid { get; set; }

        [Required]
        [BsonElement("Address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [BsonElement("Fokontany")]
        public string Fokontany { get; set; } = string.Empty;

        [Required]
        [BsonElement("District")]
        public string District { get; set; } = string.Empty;

        [Required]
        [BsonElement("Region")]
        public string Region { get; set; } = string.Empty;

        [Required]
        [BsonElement("Commune")]
        public string Commune { get; set; } = string.Empty;

        [BsonElement("Latitude")]
        public double Latitude { get; set; }

        [BsonElement("Longitude")]
        public double Longitude { get; set; }

        [BsonElement("CreatedByAgentId")]
        public string? CreatedByAgentId { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonIgnore]
        public string SyncStatus { get; set; } = "Synced";
    }
}