using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageProcess
{
    public partial class CamerPreviewWindow : Window
    {
        public CamerPreviewWindow()
        {
            InitializeComponent();
            Closing += ResultWindow_Closing;
        }

        private void ResultWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        public void SetResult(BitmapSource src, BitmapSource dst)
        {
            img_dst.Source = dst;
            img_src.Source = src;
        }

        public void UpdateFPS(double fps)
        {
            textFPS.Text = fps.ToString("F2");
        }

        public void NotifyClose()
        {
            Closing -= ResultWindow_Closing;
            Close();
        }
    }
}
