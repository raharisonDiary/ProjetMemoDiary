namespace SocialGasy.Models
{
    public class VerifyOtpRequest
    {
        public string CitizenId { get; set; } = string.Empty;
        public string CampaignId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}