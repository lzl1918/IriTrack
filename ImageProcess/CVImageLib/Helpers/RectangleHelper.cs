using System.Collections.Generic;
using System.Drawing;

namespace CVImageLib.Helpers
{
    internal static class RectangleHelper
    {
        public static Rectangle GetMaximumRectangle(IEnumerable<Rectangle> rectangles)
        {
            int x0 = 0, y0 = 0;
            int height = 0, width = 0;
            int size = 0;
            foreach (Rectangle rect in rectangles)
            {
                int singleFaceSize = rect.Width * rect.Height;
                if (size < singleFaceSize)
                {
                    x0 = rect.Left;
                    y0 = rect.Top;
                    width = rect.Width;
                    height = rect.Height;
                    size = singleFaceSize;
                }
            }
            return new Rectangle(x0, y0, width, height);
        }
    }
}
