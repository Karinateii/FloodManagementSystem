using System.ComponentModel.DataAnnotations.Schema;

namespace NewLagosFloodDetectionSystem.Models
{
    public class CsvFile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte[] Bytes { get; set; }
        public string Path { get; set; }
        //[ForeignKey("City")]
        //public int CityId { get; set; }
        //public City City { get; set; }
        public DateTime UploadDateTime { get; set; }
        public List<CsvFileCity> CsvFileCities { get; set; }
        //public ICollection<City> Cities { get; set; }
        //public string FilePath { get; set; }
        //public long Size { get; set; }
        //public string Extension { get; set; }
    }
}
