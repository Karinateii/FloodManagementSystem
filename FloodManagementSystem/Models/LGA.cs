using System.ComponentModel.DataAnnotations;

namespace GlobalDisasterManagement.Models
{
    public class LGA
    {
        public int LGAId { get; set; }
        
        [Required(ErrorMessage = "LGA name is required")]
        [StringLength(100, ErrorMessage = "LGA name cannot exceed 100 characters")]
        public string LGAName { get; set; }
        
        public ICollection<City> Cities { get; set; }
    }
}
