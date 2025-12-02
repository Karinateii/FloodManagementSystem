namespace GlobalDisasterManagement.Models.DTO.Analytics
{
    /// <summary>
    /// ML prediction accuracy and model performance
    /// </summary>
    public class PredictionAccuracyDto
    {
        public string ModelName { get; set; } = "Flood Prediction Model";
        public string ModelType { get; set; } = "Fast Tree Binary Classifier";
        public DateTime LastTrainingDate { get; set; }
        
        // Accuracy metrics
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }
        public double AucScore { get; set; } // Area Under Curve
        
        // Prediction statistics
        public int TotalPredictions { get; set; }
        public int CorrectPredictions { get; set; }
        public int FalsePositives { get; set; }
        public int FalseNegatives { get; set; }
        public int TruePositives { get; set; }
        public int TrueNegatives { get; set; }
        
        public List<PredictionByLocationDto> PredictionsByLocation { get; set; } = new();
        public List<TimeSeriesDataDto> AccuracyTrend { get; set; } = new();
    }

    /// <summary>
    /// Prediction accuracy breakdown by location
    /// </summary>
    public class PredictionByLocationDto
    {
        public string Location { get; set; } = string.Empty;
        public int TotalPredictions { get; set; }
        public double Accuracy { get; set; }
        public int CorrectPredictions { get; set; }
        public int IncorrectPredictions { get; set; }
    }
}
