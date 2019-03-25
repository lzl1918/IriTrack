using PointAngleCS.Pattern;
using PointAngleCS.Point;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointAngleCS.Processor
{
    public interface IProcessor
    {
        ProcessResult Process(IReadOnlyList<Coord> coords, PatternArguments pattern);
    }
}
