namespace PointAngleCS.Prediction
{
    public sealed class PredictionRecord
    {
        public PredictionRecord(int predictedValue, int times)
        {
            PredictedValue = predictedValue;
            Times = times;
        }

        public int PredictedValue { get; }
        public int Times { get; }
    }
}
