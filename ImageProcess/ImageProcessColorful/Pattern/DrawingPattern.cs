using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcess.Pattern
{
    public sealed class DrawingPattern
    {

        public DrawingPattern(int[] lineLengths, int[] angles)
        {
            LineLengths = lineLengths.Select(x => (double)x).ToArray();
            Angles = angles.Select(x => (double)x).ToArray();
            TotalLength = LineLengths.Sum();
        }

        public double[] LineLengths { get; set; }
        public double[] Angles { get; set; }
        public double TotalLength { get; set; }

        public PatternDrawingParameters GenerateDrawingParameters(double offsetX, double offsetY)
        {
            int angleCount = Angles.Length;
            int pcs = angleCount + 2;
            Point[] points = new Point[angleCount + 2];
            double line = LineLengths[0];
            double lastX = offsetX;
            double lastY = offsetY;
            Point tmp;
            double vecDiff = 0;
            points[0] = new Point(offsetX, offsetY);
            points[1] = new Point(offsetX, offsetY + line);
            if (pcs >= 3)
            {
                double t_angle = 3.1415926 / 180 * (Angles[0]);
                tmp = new Point();
                line = LineLengths[1];
                tmp.X = points[1].X + line * Math.Sin(t_angle);
                tmp.Y = points[1].Y - line * Math.Cos(t_angle);
                points[2] = tmp;
                vecDiff = 180 - Angles[0];
            }
            for (int i = 3; i < pcs; i++)
            {
                vecDiff = Angles[i - 2] - (180 - vecDiff);
                double t_angle = 3.1415926 / 180 * (vecDiff);
                tmp = new Point();
                line = LineLengths[i - 1];
                tmp.X = points[i - 1].X + line * Math.Sin(t_angle);
                tmp.Y = points[i - 1].Y + line * Math.Cos(t_angle);
                points[i] = tmp;
            }
            double minX = points.Min(p => p.X);
            double minY = points.Min(p => p.Y);
            minX = offsetX - minX;
            minY = offsetY - minY;
            return new PatternDrawingParameters(
                (from p in points select new Point(p.X + minX, p.Y + minY)).ToArray()
            );
        }
    }

    public sealed class PatternDrawingParameters
    {
        public PatternDrawingParameters(Point[] points)
        {
            Points = points;
        }

        public Point[] Points { get; set; }


    }

}
