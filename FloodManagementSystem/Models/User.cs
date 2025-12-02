using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GlobalDisasterManagement.Models
{
    public class User : IdentityUser
    {
        public int? LGAId { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public string? LGAName { get; set; }
    }
}
