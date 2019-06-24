using System.Diagnostics;

namespace PointAngleCS.Point
{
    [DebuggerDisplay("[{TimeOffset}: {X}, {Y}]")]
    public sealed class Coord
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double TimeOffset { get; set; }
        public Coord(double x, double y, double timeOffset)
        {
            X = x;
            Y = y;
            TimeOffset = timeOffset;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is Coord oc)
            {
                return oc.X.Equals(X) && oc.Y.Equals(Y) && oc.TimeOffset.Equals(TimeOffset);
            }
            else
                return false;
        }
        public bool EqualsIgnoringTime(Coord coord)
        {
            if (coord == null)
                return false;
            return coord.X.Equals(X) && coord.Y.Equals(Y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ TimeOffset.GetHashCode();
        }
    }
}
