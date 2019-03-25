using PointAngleCS.Pattern;
using PointAngleCS.Point;
using PointAngleCS.Prediction;
using System.Collections.Generic;

namespace PointAngleCS.Processor
{
    public sealed class ProcessResult
    {
        public ProcessResult(IReadOnlyList<Coord> coords, PatternArguments pattern, IReadOnlyList<AnglePrediction> angles, IReadOnlyList<Coord> coordPredictions)
        {
            Coords = coords;
            Pattern = pattern;
            Angles = angles;
            CoordPredictions = coordPredictions;
        }

        public IReadOnlyList<Coord> Coords { get; }
        public PatternArguments Pattern { get; }
        public IReadOnlyList<AnglePrediction> Angles { get; }
        public IReadOnlyList<Coord> CoordPredictions { get; }
    }
}
