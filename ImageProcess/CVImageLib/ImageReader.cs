using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CVImageLib
{
    public class ImageReader : IDisposable
    {
        private Mat rgbMat;
        private Mat grayMat;
        private int width;
        private int height;
        private int stride;
        private int size;

        public Mat GrayMat => grayMat;
        public int Width => width;
        public int Height => height;

        public ImageReader(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.stride = width * 4;
            this.size = stride * height;
            rgbMat = new Mat(height, width, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
            grayMat = new Mat(height, width, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
        }

        public void UpdateImage(BitmapSource image, bool equalizeHist = false)
        {
            image.CopyPixels(Int32Rect.Empty, rgbMat.DataPointer, size, stride);
            CvInvoke.CvtColor(rgbMat, grayMat, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
            CvInvoke.Flip(grayMat, grayMat, Emgu.CV.CvEnum.FlipType.Horizontal);
            CvInvoke.Flip(grayMat, grayMat, Emgu.CV.CvEnum.FlipType.Vertical);
            if (equalizeHist)
                CvInvoke.EqualizeHist(grayMat, grayMat);

        }

        public void Dispose()
        {
            rgbMat.Dispose();
            grayMat.Dispose();
        }
    }
}
