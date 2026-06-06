using Microsoft.AspNetCore.Identity;

namespace TradePlatform.Api.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsTradesperson { get; set; }
        public string CompanyName { get; set; }
        public string TradeSummary { get; set; }
        public string ProfilePhotoUrl { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
    }
}
