using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImageProcess
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string VideoPath { get; private set; }
        public static bool ShouldEqualizeHist { get; private set; }
        public static string PatternPath { get; private set; }
        public static int Speed { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            ShouldEqualizeHist = e.Args.Length <= 0 ? false : bool.Parse(e.Args[0]);
            VideoPath = e.Args.Length <= 1 ? null : e.Args[1];
            if (VideoPath == "null")
                VideoPath = null;

            PatternPath = e.Args.Length <= 2 ? null : e.Args[2];
            if (PatternPath == null)
                PatternPath = @"C:\Users\Hake\Desktop\IriTrack\figexp\pattern.txt";
            Speed = e.Args.Length <= 3 ? 200 : int.Parse(e.Args[3]);

            base.OnStartup(e);
        }
    }
}
