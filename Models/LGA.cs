using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewLagosFloodDetectionSystem.Models
{
    public class LGA
    {
        public int LGAId { get; set; }
        public string LGAName { get; set; }
        public ICollection<City> Cities { get; set; }
    }
}
