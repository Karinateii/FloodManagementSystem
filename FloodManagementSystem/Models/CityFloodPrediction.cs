namespace GlobalDisasterManagement.Models
{
    public class CityFloodPrediction
    {
        public int Id { get; set; }
        public Guid FileId { get; set; }
        public Guid ModelId { get; set; }
        public string ModelFileName { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string Prediction { get; set; }
    }
}
