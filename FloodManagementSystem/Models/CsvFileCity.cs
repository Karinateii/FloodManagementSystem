using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    public class CsvFileCity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public int CityId { get; set; }
        public Guid ModelId { get; set; }
        public string Year { get; set; }
        public string Name { get; set; }
        public byte[] Bytes { get; set; }
        public string Path { get; set; }
        //[ForeignKey("City")]
        //public int CityId { get; set; }
        //public City City { get; set; }
        public DateTime UploadDateTime { get; set; }
    }
}
