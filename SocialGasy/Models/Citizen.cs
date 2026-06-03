using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace SocialGasy.Models
{
    // Ity attribute ity no manala ilay "FormatException" 
    // satria izy "tsy miraharaha" ny saha tsy hita ao amin'ny Model
    [BsonIgnoreExtraElements] 
    public class Citizen
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("CIN")]
        public string CIN { get; set; } = string.Empty;

        [BsonElement("Nom")] 
        public string LastName { get; set; } = string.Empty;

        [BsonElement("Prenom")]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("DateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [BsonElement("Gender")]
        public string Gender { get; set; } = string.Empty;

        [BsonElement("MaritalStatus")]
        public string MaritalStatus { get; set; } = string.Empty;

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