using Microsoft.ML.Data;

namespace NewLagosFloodDetectionSystem.Models
{
    public class FloodPredictionResult
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        [ColumnName("Probability")]
        public float Probability { get; set; }

        [ColumnName("Score")]
        public float Score { get; set; }        
        //[ColumnName("PredictedLabel")]
        //public uint PredictedClusterId;
        //[ColumnName("PredictedLabel")]
        //public bool IsFloodProne { get; set; }

        //[ColumnName("Probability")]
        //public float Prediction { get; set; }


        //[ColumnName("Score")]
        //public float[] Prediction { get; set; }

        ////public string City { get; set; }
        ////public int Month { get; set; }
        ////public int Year { get; set; }
        ////public City City;
        //[ColumnName("PredictedLabel")]
        //public bool PredictedFloodRisk { get; set; }
    }
}
