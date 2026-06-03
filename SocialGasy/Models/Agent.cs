using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SocialGasy.Models
{
    public class Agent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty; 

        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = "Agent"; 

        public string? LoginToken { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}