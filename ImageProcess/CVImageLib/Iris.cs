#define NEWMETHOD

using CVImageLib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVImageLib
{
    public static class Iris
    {
        private static double[] PIXEL_ANGS;
        private static double[] PIXEL_COS_VALUES;
        private static double[] PIXEL_SIN_VALUES;
        private static int PIXEL_ANGLES_COUNT;

        private static double[] SECTION_ANGS;
        private static double[] SECTION_COS_VALUES;
        private static double[] SECTION_SIN_VALUES;
        private static int SECTION_ANGLES_COUNT;

        private static int minrad = 3;
        private static int maxrad = 45;
        private static int jump = 5;

        private static int PROCESSOR_CORE_COUNT;

        static Iris()
        {
            PROCESSOR_CORE_COUNT = Environment.ProcessorCount;

            if (File.Exists("config.txt"))
            {
                FileStream configStream = File.OpenRead("config.txt");
                StreamReader reader = new StreamReader(configStream);
                string line;
                line = reader.ReadLine();
                minrad = int.Parse(line);
                line = reader.ReadLine();
                maxrad = int.Parse(line);
                line = reader.ReadLine();
                jump = int.Parse(line);
            }

            double prec = 0.5;
            double PI2 = Math.PI * 2;
            List<double> angs = new List<double>();
            {
                double ang = 0;
                while (ang < PI2)
                {
                    angs.Add(ang);
                    ang += prec;
                }
                SECTION_ANGS = angs.ToArray();
                SECTION_COS_VALUES = angs.Select(a => Math.Cos(a)).ToArray();
                SECTION_SIN_VALUES = angs.Select(a => Math.Sin(a)).ToArray();
                SECTION_ANGLES_COUNT = SECTION_ANGS.Length;
            }

            prec = 0.1;
            angs.Clear();
            {
                double ang = 0;
                while (ang < PI2)
                {
                    angs.Add(ang);
                    ang += prec;
                }
                PIXEL_ANGS = angs.ToArray();
                PIXEL_COS_VALUES = angs.Select(a => Math.Cos(a)).ToArray();
                PIXEL_SIN_VALUES = angs.Select(a => Math.Sin(a)).ToArray();
                PIXEL_ANGLES_COUNT = PIXEL_ANGS.Length;
            }
        }

        private static double SectionCountourIntegralCircular(byte[] img, int width, int height, int y0, int x0, int r)
        {
            double sum = 0;
            int y, x;
            for (int index = 0; index < SECTION_ANGLES_COUNT; index++)
            {
                y = (int)Math.Round(y0 - SECTION_COS_VALUES[index] * r);
                x = (int)Math.Round(x0 + SECTION_SIN_VALUES[index] * r);
                if (y < 0)
                    y = 0;
                else if (y >= height)
                    y = height - 1;
                if (x < 0)
                    x = 0;
                else if (x >= width)
                    x = width - 1;
                sum += img[y * width + x];
            }
            return sum;
        }
        private static double PixelCountourIntegralCircular(byte[] img, int width, int height, int y0, int x0, int r)
        {
            double sum = 0;
            int y, x;
            for (int index = 0; index < PIXEL_ANGLES_COUNT; index++)
            {
                y = (int)Math.Round(y0 - PIXEL_COS_VALUES[index] * r);
                x = (int)Math.Round(x0 + PIXEL_SIN_VALUES[index] * r);
                if (y < 0)
                    y = 0;
                else if (y >= height)
                    y = height - 1;
                if (x < 0)
                    x = 0;
                else if (x >= width)
                    x = width - 1;
                sum += img[y * width + x];
            }
            return sum;
        }

        public static Circle SearchInnerBoundary(byte[] img, int width, int height, int x0, int y0, int x1, int y1)
        {
            int area_width = x1 - x0;
            int area_height = y1 - y0;
            //int total = Math.Min(area_width, area_height);
            //int minrad = (int)(total * 0.3);
            //int maxrad = (int)(total * 0.7);
            //int jump = Math.Max((maxrad - minrad) / 4, 4);
            //Console.WriteLine($"{total}, {minrad}, {maxrad}, {jump}");

            int search_y = (int)Math.Floor((double)area_height / jump);
            int search_x = (int)Math.Floor((double)area_width / jump);
            int search_r = (int)Math.Floor((double)(maxrad - minrad) / jump);
            //Console.WriteLine($"sh: {search_x}, {search_y}, {search_r}");

            double[,,] hs = new double[search_y, search_x, search_r];

            int xval = x0, yval, rval;
            for (int x = 0; x < search_x; x++)
            {
                yval = y0;
                for (int y = 0; y < search_y; y++)
                {
                    rval = minrad;
                    for (int r = 0; r < search_r; r++)
                    {
                        hs[y, x, r] = SectionCountourIntegralCircular(img, width, height, yval, xval, rval);
                        rval += jump;
                    }
                    yval += jump;
                }
                xval += jump;
            }

            double[,,] hspdr = new double[search_y, search_x, search_r];
            for (int y = 0; y < search_y; y++)
                for (int x = 0; x < search_x; x++)
                    for (int r = 1; r < search_r; r++)
                        hspdr[y, x, r] = hs[y, x, r] - hs[y, x, r - 1];

#if NEWMETHOD
            Circle roughSearch = ConvMax(hspdr, search_y, search_x, search_r, 3);
#else
            Circle roughSearch = OriginalConvMax(hspdr, search_y, search_x, search_r, 3);
#endif
            int maxy = y0 + roughSearch.Y * jump;
            int maxx = x0 + roughSearch.X * jump;
            int maxr = minrad + (roughSearch.Radius - 1) * jump;

            search_x = search_y = search_r = jump * 2;
            hs = new double[search_y, search_x, search_r];
            xval = maxx - jump;
            for (int x = 0; x < search_x; x++)
            {
                yval = maxy - jump;
                for (int y = 0; y < search_y; y++)
                {
                    rval = maxr - jump;
                    for (int r = 0; r < search_r; r++)
                    {
                        hs[y, x, r] = PixelCountourIntegralCircular(img, width, height, yval, xval, rval);
                        rval++;
                    }
                    yval++;
                }
                xval++;
            }


            hspdr = new double[search_y, search_x, search_r];
            for (int y = 0; y < search_y; y++)
                for (int x = 0; x < search_x; x++)
                {
                    hspdr[y, x, 0] = 0; // hs[y, x, 0] - hs[y, x, 1];
                    for (int r = 1; r < search_r; r++)
                        hspdr[y, x, r] = hs[y, x, r] - hs[y, x, r - 1];
                }
#if NEWMETHOD
            Circle search = ConvMax(hspdr, search_y, search_x, search_r, 3);
#else
            Circle search = OriginalConvMax(hspdr, search_y, search_x, search_r, 3);
#endif
            maxy = maxy - jump + search.Y;
            maxx = maxx - jump + search.X;
            maxr = maxr - jump + search.Radius - 1;
            return new Circle(maxx, maxy, maxr);
        }

        private static ParallelConv parallelConv = new ParallelConv();
        private static Circle ConvMax(double[,,] hspdr, int y, int x, int r, int masksize)
        {
            return parallelConv.Conv(hspdr, y, x, r, masksize);
        }

        private static Circle OriginalConvMax(double[,,] hspdr, int y, int x, int r, int masksize)
        {
            double max = 0;
            int max_y = 0, max_x = 0, max_r = 0;

            double tmp;
            int j1min = (masksize - 1) / 2;
            int j2min = j1min;
            int j3min = j1min;
            int j1max = y + masksize - j1min - 2;
            int j2max = x + masksize - j2min - 2;
            int j3max = r + masksize - j3min - 2;
            int min1, min2, min3;
            int max1, max2, max3;

            for (int j1 = j1min; j1 <= j1max; j1++)
            {
                min1 = Math.Max(0, j1 - masksize + 1);
                max1 = Math.Min(y - 1, j1);
                for (int j2 = j2min; j2 <= j2max; j2++)
                {
                    min2 = Math.Max(0, j2 - masksize + 1);
                    max2 = Math.Min(x - 1, j2);
                    for (int j3 = j3min; j3 <= j3max; j3++)
                    {
                        min3 = Math.Max(0, j3 - masksize + 1);
                        max3 = Math.Min(r - 1, j3);

                        tmp = 0;
                        for (int k1 = min1; k1 <= max1; k1++)
                            for (int k2 = min2; k2 <= max2; k2++)
                                for (int k3 = min3; k3 <= max3; k3++)
                                    tmp += hspdr[k1, k2, k3];

                        if (tmp > max)
                        {
                            max = tmp;
                            max_y = j1 - j1min;
                            max_x = j2 - j2min;
                            max_r = j3 - j3min;
                        }
                    }
                }
            }
            return new Circle(max_x, max_y, max_r);
        }
    }

}
