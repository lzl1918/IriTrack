using ImageProcess.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ImageProcess
{

    public partial class PatternDrawWindow : Window
    {
        public const int POINT_STAY = 400;
        public const double OFFSET_X = 50;
        public const double OFFSET_Y = 10;

        private DrawingPattern pattern;
        private PatternDrawingParameters drawingParameters;

        private double[] lines;

        public event EventHandler MoveBegin
        {
            add => patternDrawingControl.MoveBegin += value;
            remove => patternDrawingControl.MoveBegin -= value;
        }
        public event EventHandler MoveEnded
        {
            add => patternDrawingControl.MoveEnded += value;
            remove => patternDrawingControl.MoveEnded -= value;
        }

        public PatternDrawingParameters DrawingParamters => drawingParameters;

        public PatternDrawWindow(PatternArguments arg, double speed = 200)
        {
            InitializeComponent();

            pattern = new DrawingPattern(arg.LineLengths, arg.Angles);
            drawingParameters = pattern.GenerateDrawingParameters(OFFSET_X, OFFSET_Y);

            lines = pattern.LineLengths;
            patternDrawingControl.SetPattern(drawingParameters, lines, speed / 1000);
        }

        public void Start()
        {
            patternDrawingControl.Start();
        }

    }
}
