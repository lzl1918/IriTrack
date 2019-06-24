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
        public static int FrameRate { get; private set; }


        protected override void OnStartup(StartupEventArgs e)
        {
            ShouldEqualizeHist = e.Args.Length <= 0 ? false : bool.Parse(e.Args[0]);
            VideoPath = e.Args.Length <= 1 ? null : e.Args[1];
            if (VideoPath == "null")
                VideoPath = null;
            FrameRate = e.Args.Length <= 2 ? 10 : int.Parse(e.Args[2]);

            base.OnStartup(e);
        }
    }
}
