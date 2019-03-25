using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CVImageLib
{
    public interface IDetector : IDisposable
    {
        void DetectFace(Mat mat, out FaceRetrieveResult face, out EyeRetrieveResult eye);
        FaceRetrieveResult FindFace(Mat image);
        EyeRetrieveResult FindEyes(Mat image, in Rectangle face);
    }

}
