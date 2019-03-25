using PointAngleCS.Pattern;
using PointAngleCS.Point;
using PointAngleCS.Prediction;
using System.Collections.Generic;
using System.Linq;

namespace PointAngleCS.Processor
{
    public sealed class TimeTagSensitiveProcessor : IProcessor
    {
        public static int SAMPLE_COUNT { get; } = 3;
        public static int HEAD_SKIP { get; } = 3;
        public static int END_SKIP { get; } = 3;

        public ProcessResult Process(IReadOnlyList<Coord> coords, PatternArguments pattern)
        {
            int[] lengthStepSum = new int[pattern.LineCount + 1];
            double lengthSum = 0;
            double[] pointScales = new double[pattern.LineCount + 1];
            double[] actualTimes = new double[pattern.LineCount + 1];
            {
                lengthStepSum[0] = 0;
                for (int i = 1; i <= pattern.LineCount; i++)
                    lengthStepSum[i] = lengthStepSum[i - 1] + pattern.LineLengths[i - 1];
                lengthSum = 1.0 * lengthStepSum[pattern.LineCount];
                for (int i = 0; i <= pattern.LineCount; i++)
                    pointScales[i] = lengthStepSum[i] / lengthSum;
            }

            // skip some points in the very beginning and ending
            List<Coord> trimmedPoints;
            {
                int headSkip = HEAD_SKIP;
                int endSkip = END_SKIP;
                trimmedPoints = coords.Skip(headSkip).Take(coords.Count - headSkip - endSkip).ToList();
            }

            // take sample points
            // get two points that are most close to the corresponding break-point by comparing timeoffset tag
            int[,] sampleIndex = new int[pattern.LineCount + 1, 4];
            {
                double baseOffset = trimmedPoints.First().TimeOffset;
                double duration = trimmedPoints.Last().TimeOffset - baseOffset;
                for (int i = 0; i <= pattern.LineCount; i++)
                    actualTimes[i] = pointScales[i] * duration + baseOffset;

                int halfSample = SAMPLE_COUNT / 2;
                int count = trimmedPoints.Count - halfSample - halfSample;
                int j = halfSample;
                bool add;
                for (int i = 0; i <= pattern.LineCount; i++)
                {
                    add = false;

                    for (; j <= count; j++)
                    {
                        if (trimmedPoints[j].TimeOffset > actualTimes[i])
                        {
                            sampleIndex[i, 0] = System.Math.Max(j - 2, 0);
                            sampleIndex[i, 1] = System.Math.Max(j - 1, 1);
                            sampleIndex[i, 2] = System.Math.Min(j, count - 1);
                            sampleIndex[i, 3] = System.Math.Min(++j, count);
                            add = true;
                            break;
                        }
                    }
                    if (!add)
                    {
                        sampleIndex[i, 0] = count - 3;
                        sampleIndex[i, 1] = count - 2;
                        sampleIndex[i, 2] = count - 1;
                        sampleIndex[i, 3] = count;
                    }

                }
            }

            // take {SAMPLE_COUNT} predicted points for each break-point
            List<List<Coord>> keyPoints = new List<List<Coord>>();
            {
                int halfSample = SAMPLE_COUNT / 2;
                List<Coord> predicts;
                double time;

                for (int i = 0; i <= pattern.LineCount; i++)
                {
                    time = actualTimes[i];
                    Coord a = Math.CalcHelper.PredictCoord(trimmedPoints[sampleIndex[i, 0]], trimmedPoints[sampleIndex[i, 1]], time);
                    Coord b = Math.CalcHelper.PredictCoord(trimmedPoints[sampleIndex[i, 2]], trimmedPoints[sampleIndex[i, 3]], time);
                    Coord c = new Coord((a.X + b.X) / 2, (a.Y + b.Y) / 2, time);
                    predicts = new List<Coord>() { a, b, c };
                    keyPoints.Add(predicts);
                }
            }
            List<Coord> predictedCoords = new List<Coord>();
            foreach (List<Coord> predictCoords in keyPoints)
            {
                predictedCoords.AddRange(predictCoords);
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
                            if (begin.EqualsIgnoringTime(mid) || mid.EqualsIgnoringTime(end))
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

            return new ProcessResult(coords, pattern, predictionResults, predictedCoords);
        }

    }
}
