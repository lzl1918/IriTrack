using PointAngleCS.Pattern;
using PointAngleCS.Point;
using PointAngleCS.Prediction;
using System.Collections.Generic;
using System.Linq;

namespace PointAngleCS.Processor
{
    public sealed class DefaultProcessor : IProcessor
    {
        public static int SAMPLE_COUNT { get; } = 3;
        public static int HEAD_SKIP { get; } = 3;
        public static int END_SKIP { get; } = 3;

        public ProcessResult Process(IReadOnlyList<Coord> points, PatternArguments pattern)
        {
            int[] lengthStepSum = new int[pattern.LineCount + 1];
            double lengthSum = 0;
            double[] pointScales = new double[pattern.LineCount + 1];
            {
                lengthStepSum[0] = 0;
                for (int i = 1; i <= pattern.LineCount; i++)
                {
                    lengthStepSum[i] = lengthStepSum[i - 1] + pattern.LineLengths[i - 1];
                }
                lengthSum = 1.0 * lengthStepSum[pattern.LineCount];
                pointScales = new double[pattern.LineCount + 1];
                for (int i = 0; i <= pattern.LineCount; i++)
                {
                    pointScales[i] = lengthStepSum[i] / lengthSum;
                }
            }

            List<Coord> trimmedPoints;
            {
                int headSkip = HEAD_SKIP;
                int endSkip = END_SKIP;
                trimmedPoints = points.Skip(headSkip).Take(points.Count - headSkip - endSkip).ToList();
            }

            int[] sampleIndex = new int[pattern.LineCount + 1];
            {
                int halfSample = SAMPLE_COUNT / 2;
                int count = trimmedPoints.Count - halfSample - halfSample;
                for (int i = 0; i <= pattern.LineCount; i++)
                {
                    sampleIndex[i] = (int)System.Math.Floor(pointScales[i] * count + halfSample);
                }
            }

            List<List<Coord>> keyPoints = new List<List<Coord>>();
            {
                int halfSample = SAMPLE_COUNT / 2;
                foreach (int index in sampleIndex)
                {
                    keyPoints.Add(trimmedPoints.Skip(index - halfSample).Take(SAMPLE_COUNT).ToList());
                }
            }
            List<Coord> predictedCoords = new List<Coord>();
            foreach (List<Coord> coords in keyPoints)
            {
                predictedCoords.AddRange(coords);
            }
            Dictionary<int, int> anglePredicts = new Dictionary<int, int>();
            AnglePrediction prediction;
            List<AnglePrediction> predictionResults = new List<AnglePrediction>();
            List<PredictionRecord> angleRecords;
            double ang;
            int angleInt;
            for (int i = 1; i < keyPoints.Count - 1; i++)
            {
                anglePredicts.Clear();
                foreach (Coord begin in keyPoints[i - 1])
                {
                    foreach (Coord mid in keyPoints[i])
                    {
                        foreach (Coord end in keyPoints[i + 1])
                        {
                            if (begin.Equals(mid) && mid.Equals(end))
                                continue;

                            ang = Math.CalcHelper.GetAngle(begin, mid, end);
                            angleInt = (int)ang;
                            if (angleInt == int.MaxValue || angleInt == int.MinValue)
                                continue;

                            if (anglePredicts.ContainsKey(angleInt))
                                anglePredicts[angleInt]++;
                            else
                                anglePredicts[angleInt] = 1;
                        }
                    }
                }

                angleRecords = new List<PredictionRecord>();
                foreach (var pair in anglePredicts)
                    angleRecords.Add(new PredictionRecord(pair.Key, pair.Value));
                prediction = new AnglePrediction(pattern.Angles[i - 1], angleRecords);
                predictionResults.Add(prediction);
            }

            return new ProcessResult(points, pattern, predictionResults, predictedCoords);
        }
    }
}
