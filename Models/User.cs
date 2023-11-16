using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewLagosFloodDetectionSystem.Models
{
    public class User : IdentityUser
    {
        [Required]
        public int LGAId { get; set; }
        [Required]
        public int CityId { get; set; }
        public string City{ get; set; }
        public string LGA { get; set; }
        public City city { get; set; }

    }
}
