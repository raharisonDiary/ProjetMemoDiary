using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace SocialGasy.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("Password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("Role")]
        public string Role { get; set; } = "Agent";

        [BsonElement("FullName")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("Email")]
        public string Email { get; set; } = string.Empty;

        // Ireo saha ilaina ao amin'ny View CreateChef.cshtml
        [BsonElement("Cin")]
        public string? Cin { get; set; }

        [BsonElement("PhoneNumber")]
        public string? PhoneNumber { get; set; }

        [BsonElement("Address")]
        public string? Address { get; set; }

        [BsonElement("LoginToken")]
        public string? LoginToken { get; set; }

        [BsonElement("QrCodeImage")]
        public byte[]? QrCodeImage { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}