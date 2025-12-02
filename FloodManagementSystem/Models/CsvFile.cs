using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalDisasterManagement.Models
{
    public class CsvFile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        
        [Required]
        public byte[] Bytes { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Path { get; set; }
        
        public DateTime UploadDateTime { get; set; }
        
        public List<CsvFileCity> CsvFileCities { get; set; }
    }
}
