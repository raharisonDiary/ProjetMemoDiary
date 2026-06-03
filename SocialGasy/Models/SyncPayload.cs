using System.Collections.Generic;

namespace SocialGasy.Models
{
    public class SyncPayload
    {
        public List<Household> Households { get; set; } = new List<Household>();
        public List<Citizen> Citizens { get; set; } = new List<Citizen>();
    }
}