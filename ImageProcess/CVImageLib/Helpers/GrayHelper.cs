using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVImageLib.Helpers
{
    public class GrayHelper
    {
        public static void DrawRectLocal(byte[] img, int width, int height, int x0, int y0, int x1, int y1)
        {
            int index;
            int rindex;

            index = y0 * width + x0;
            for (int i = x0; i <= x1; i++)
                img[index++] = 255;
            index = (y0 + 1) * width + x0;
            for (int i = x0; i <= x1; i++)
                img[index++] = 255;

            index = y1 * width + x0;
            for (int i = x0; i <= x1; i++)
                img[index++] = 255;
            index = (y1 - 1) * width + x0;
            for (int i = x0; i <= x1; i++)
                img[index++] = 255;

            rindex = y0 * width;
            for (int i = y0; i <= y1; i++)
            {
                index = rindex + x0;
                img[index] = 255;
                img[index + 1] = 255;
                index = rindex + x1;
                img[index] = 255;
                img[index - 1] = 255;
                rindex += width;
            }
        }

        public static byte[] DrawRect(byte[] img, int width, int height, int x0, int y0, int x1, int y1)
        {
            byte[] result = new byte[height * width];
            Array.Copy(img, result, result.Length);
            DrawRectLocal(result, width, height, x0, y0, x1, y1);
            return result;
        }

        public static void DrawCircleLocal(byte[] img, int width, int height, int x0, int y0, int r)
        {
            double prec = 0.01;
            double PI2 = Math.PI * 2;
            List<double> angs = new List<double>();
            {
                double ang = 0;
                while (ang < PI2)
                {
                    angs.Add(ang);
                    ang += prec;
                }
            }
            int y, x;
            foreach (double ang in angs)
            {
                y = (int)Math.Round(y0 - Math.Cos(ang) * r);
                x = (int)Math.Round(x0 + Math.Sin(ang) * r);
                if (y < 1)
                    y = 1;
                else if (y >= height)
                    y = height - 1;
                if (x < 1)
                    x = 1;
                else if (x >= width)
                    x = width - 1;
                img[y * width + x] = 255;
            }
            DrawPointLocal(img, width, height, x0, y0);
        }
        public static byte[] DrawCircle(byte[] img, int width, int height, int x0, int y0, int r)
        {
            byte[] result = new byte[height * width];
            Array.Copy(img, result, result.Length);
            DrawCircleLocal(result, width, height, y0, x0, r);
            return result;
        }

        public static void DrawPointLocal(byte[]img, int width, int height, int x, int y)
        {
            int rindex = y * width;
            int index;
            for (int i = 0; i < 3; i++)
            {
                index = rindex + x - 1;
                if (index < rindex)
                    index = rindex;
                img[index++] = 255;
                img[index++] = 255;
                img[index++] = 255;
                rindex += width;
            }

        }
    }
}
