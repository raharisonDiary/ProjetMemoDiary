using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialGasy.Models
{
    public class Stock
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string ItemName { get; set; } = string.Empty; 
        public string BatchNumber { get; set; } = string.Empty; 
        
        public double Quantity { get; set; } 
        public string Unit { get; set; } = "kg";

        public string Fokontany { get; set; } = string.Empty; 
        
        public DateTime ExpiryDate { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}