using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SocialGasy.Models
{
    [BsonIgnoreExtraElements]
    public class Citizen
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? ClientGuid { get; set; }

        [BsonElement("CIN")]
        public string CIN { get; set; } = string.Empty;

        [BsonElement("LastName")]
        public string LastName { get; set; } = string.Empty;

        [BsonElement("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("DateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [BsonElement("Gender")]
        public string Gender { get; set; } = string.Empty;

        [BsonElement("MaritalStatus")]
        public string MaritalStatus { get; set; } = string.Empty;

        [BsonElement("SpouseName")]
        public string? SpouseName { get; set; }

        [BsonElement("NumberOfChildren")]
        public int? NumberOfChildren { get; set; }

        [BsonElement("Profession")]
        public string? Profession { get; set; }

        [BsonElement("HouseholdId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string HouseholdId { get; set; } = string.Empty;

        public string? PhotoBase64 { get; set; }
        public string? QRCodeData { get; set; }
        public string? PhoneNumber { get; set; }

        [BsonElement("RegisteredAt")]
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        [BsonIgnore]
        public string SyncStatus { get; set; } = "Synced";
    }
}