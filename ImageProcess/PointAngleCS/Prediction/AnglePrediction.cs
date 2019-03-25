using System.Collections.Generic;

namespace PointAngleCS.Prediction
{
    public sealed class AnglePrediction
    {
        public AnglePrediction(int actualAngle, IReadOnlyList<PredictionRecord> predictions)
        {
            ActualAngle = actualAngle;
            Predictions = predictions;

            MostPossiblePrediction = GetPredictionOfMaximumTimes();
        }

        public int ActualAngle { get; }
        public IReadOnlyList<PredictionRecord> Predictions { get; }
        public PredictionRecord MostPossiblePrediction { get; }

        private PredictionRecord GetPredictionOfMaximumTimes()
        {
            int maxTimes = int.MinValue;
            int minDiff = int.MaxValue;
            int diff;
            PredictionRecord maxRecord = null;
            foreach (PredictionRecord record in Predictions)
            {
                if (record.Times > maxTimes)
                {
                    maxTimes = record.Times;
                    maxRecord = record;
                    minDiff = System.Math.Abs(maxRecord.PredictedValue - ActualAngle);
                }
                else if (record.Times == maxTimes &&
                        (diff = System.Math.Abs(record.PredictedValue - ActualAngle)) < minDiff)
                {
                    maxRecord = record;
                    minDiff = diff;
                }
            }
            return maxRecord;
        }
    }
}
