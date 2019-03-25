using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVImageLib
{
    public class EyeSearchRegionExtractor
    {
        public void GetSearchRegion(in Rectangle faceRegion, out Rectangle leftEyeRegion, out Rectangle rightEyeRegion)
        {
            int width = faceRegion.Width;
            int height = faceRegion.Height;
            int eyeWidth = (int)(width * 0.4);
            int eyeHeight = (int)(height * 0.3);
            leftEyeRegion = new Rectangle((int)(width * 0.10), (int)(height * 0.2), eyeWidth, eyeHeight);
            rightEyeRegion = new Rectangle((int)(width * 0.5), (int)(height * 0.2), eyeWidth, eyeHeight);
        }
    }
}
