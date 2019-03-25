using PointAngleCS.Pattern;
using PointAngleCS.Point;
using PointAngleCS.Processor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointAngleCS
{
    public class Program
    {
        private static List<Coord>[] ReadPoints(string path)
        {
            Stream file = File.OpenRead(path);
            StreamReader reader = new StreamReader(file);
            string content;
            string eye;
            string[] spt;
            double x, y, time;
            List<Coord> leftCoords = new List<Coord>();
            List<Coord> rightCoords = new List<Coord>();
            List<Coord>[] coords = new List<Coord>[2] { leftCoords, rightCoords };
            while ((content = reader.ReadLine()) != null)
            {
                spt = content.Split(',');
                eye = spt[0];
                time = double.Parse(spt[1]);
                x = double.Parse(spt[2]);
                y = double.Parse(spt[3]);
                if (eye == "l")
                    leftCoords.Add(new Coord(x, y, time));
                else if (eye == "r")
                    rightCoords.Add(new Coord(x, y, time));
            }
            return coords;
        }
        private static void WriteResult(ProcessResult result)
        {
            // write predictions
            {
                foreach (var angle in result.Angles)
                {
                    Console.WriteLine($"[{angle.ActualAngle}]:");
                    foreach (var measure in angle.Predictions.OrderByDescending(m => m.Times))
                        Console.WriteLine($"{measure.PredictedValue:d3}, {measure.Times}");
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            // write selected value
            {
                int count = result.Pattern.AngleCount;
                int actual;
                int predict;
                int weight;
                for (int i = 0; i < count; i++)
                {
                    actual = result.Pattern.Angles[i];
                    predict = result.Angles[i].MostPossiblePrediction.PredictedValue;
                    weight = result.Pattern.Weights[i];
                    Console.WriteLine($"[{actual:d3}]{predict:d3}, {weight}");
                }
                Console.WriteLine();
            }
        }
        private static double GetMatchCost(ProcessResult processResult)
        {
            int count = processResult.Pattern.AngleCount;
            int diffSum = 0;
            int actual;
            int predict;
            int weight;
            int weightSum = processResult.Pattern.Weights.Sum();
            for (int i = 0; i < count; i++)
            {
                actual = processResult.Pattern.Angles[i];
                predict = processResult.Angles[i].MostPossiblePrediction.PredictedValue;
                weight = processResult.Pattern.Weights[i];
                diffSum += weight * System.Math.Abs(predict - actual);
            }
            return diffSum * 1.0 / weightSum;
        }
        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Argument Error");
                return;
            }

            string patternPath = args[0];
            string pointsPath = args[1];
            bool useDefault = bool.Parse(args[2]);

            PatternArguments pattern = PatternArguments.ReadFromFile(patternPath);
            List<Coord>[] eyePoints = ReadPoints(pointsPath);

            IProcessor processor;
            if (useDefault)
                processor = new DefaultProcessor();
            else
                processor = new TimeTagSensitiveProcessor();

            DateTime startTime = DateTime.Now;
            ProcessResult result = processor.Process(eyePoints[0], pattern);
            double cost = GetMatchCost(result);
            WriteResult(result);
            Console.WriteLine(cost);
            Console.WriteLine("======");

            result = processor.Process(eyePoints[1], pattern);
            cost = GetMatchCost(result);
            DateTime endTime = DateTime.Now;
            WriteResult(result);
            Console.WriteLine(cost);
            Console.WriteLine((endTime - startTime).TotalMilliseconds);
            Console.WriteLine();
            foreach(Coord coord in result.CoordPredictions)
            {
                Console.WriteLine("{0}, {1}", coord.X, coord.Y);
            }
        }
    }
}
