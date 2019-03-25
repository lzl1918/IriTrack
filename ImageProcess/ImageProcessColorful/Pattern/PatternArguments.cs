using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcess.Pattern
{
    public class PatternArguments
    {
        public int AngleCount { get; }
        public int LineCount { get; }

        public int[] Angles { get; }
        public int[] Weights { get; }
        public int[] LineLengths { get; }

        private PatternArguments(string path)
        {
            FileStream pointStream = File.OpenRead(path);
            StreamReader reader = new StreamReader(pointStream);
            int angleCount = int.Parse(reader.ReadLine());
            reader.ReadLine();
            int[] angles = new int[angleCount];
            for (int i = 0; i < angleCount; i++)
            {
                angles[i] = int.Parse(reader.ReadLine());
            }
            reader.ReadLine();

            int[] weights = new int[angleCount];
            for (int i = 0; i < angleCount; i++)
            {
                weights[i] = int.Parse(reader.ReadLine());
            }
            reader.ReadLine();

            int lineCount = angleCount + 1;
            int[] lines = new int[lineCount];
            for (int i = 0; i < lineCount; i++)
            {
                lines[i] = int.Parse(reader.ReadLine());
            }
            reader.Dispose();
            pointStream.Dispose();

            this.AngleCount = angleCount;
            this.LineCount = lineCount;
            this.Angles = angles;
            this.Weights = weights;
            this.LineLengths = lines;
        }

        public static PatternArguments ReadFromFile(string path)
        {
            return new PatternArguments(path);
        }
    }

}
