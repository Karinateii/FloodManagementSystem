
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewLagosFloodDetectionSystem.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public float Latitude { get; set; }
        //public float Longitude { get; set; }
        public int LGAId { get; set; }
        //public int SelectedLGA { get; set; }
        //public IEnumerable<SelectListItem> LGAs { get; set; }
        public LGA LGA { get; set; }
        public ICollection<CityFloodPrediction> Predictions { get; set; }

        //public ICollection<FloodDataModel> FloodData { get; set; }
        //public CsvFileModel CsvFile { get; set; }

    }
}
