using PointAngleCS.Point;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointAngleCS.Math
{
    public static class CalcHelper
    {
        private readonly static double DIVIDE_180_WITH_PI = 180 / System.Math.PI;
        public static double GetAngle(Coord begin, Coord mid, Coord end)
        {
            double deltaX1 = mid.X - begin.X;
            double deltaY1 = mid.Y - begin.Y;
            double deltaX2 = mid.X - end.X;
            double deltaY2 = mid.Y - end.Y;
            double vectorProduct = deltaX1 * deltaX2 + deltaY1 * deltaY2;
            double cos = vectorProduct / (System.Math.Sqrt(deltaX1 * deltaX1 + deltaY1 * deltaY1) * System.Math.Sqrt(deltaX2 * deltaX2 + deltaY2 * deltaY2));
            double ang = System.Math.Acos(cos);

            return DIVIDE_180_WITH_PI * ang;
        }

        public static Coord PredictCoord(Coord a, Coord b, double actualTime)
        {
            if (a.TimeOffset > b.TimeOffset)
                return PredictCoord(b, a, actualTime);
            if (a.EqualsIgnoringTime(b))
                return new Coord(a.X, a.Y, actualTime);

            double vecX = b.X - a.X;
            double vecY = b.Y - a.Y;
            double timeOffset = b.TimeOffset - a.TimeOffset;
            double actualOffset = actualTime - a.TimeOffset;
            vecX = vecX / timeOffset * actualOffset;
            vecY = vecY / timeOffset * actualOffset;
            return new Coord(a.X + vecX, a.Y + vecY, actualTime);
        }

    }
}
