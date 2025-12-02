using System.ComponentModel.DataAnnotations;

namespace GlobalDisasterManagement.Models
{
    public class City
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "City name is required")]
        [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters")]
        public string Name { get; set; }
        
        [Required]
        public int LGAId { get; set; }
        
        public LGA LGA { get; set; }
        
        public ICollection<CityFloodPrediction> Predictions { get; set; }
    }
}
