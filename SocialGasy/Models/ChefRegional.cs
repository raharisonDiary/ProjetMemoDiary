using System;
using System.ComponentModel.DataAnnotations;

namespace SocialGasy.Models
{
    public class ChefRegional
    {
        public string? Id { get; set; }
        [Required] public string LastName { get; set; } = string.Empty;
        [Required] public string FirstName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        [Required] public string CIN { get; set; } = string.Empty;
        [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty; // Teny miafina
        public string Role { get; set; } = "ChefRegional";
        public string? Region { get; set; } // Faritra iandraiketana
    }
}