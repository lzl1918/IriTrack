using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace CVImageLib.Helpers
{
    internal class ParallelConv
    {
        private class Context
        {
            public double[,,] hspdr;
            public int maskSize;
            public int yStart;
            public int yEnd;
            public int xStart;
            public int xEnd;
            public int r;

            public double sum;
            public Circle result;
        }

        private Context Conv(object arg)
        {
            Context context = (Context)arg;
            double max = 0;
            int max_y = 0, max_x = 0, max_r = 0;

            double tmp;
            int j1max = context.yEnd;
            int j2max = context.xEnd;
            int j3max = context.r;

            int max1 = context.yStart + context.maskSize - 1, max2, max3;
            for (int j1 = context.yStart; j1 <= j1max; j1++, max1++)
            {
                max2 = context.xStart + context.maskSize - 1;
                for (int j2 = context.xStart; j2 <= j2max; j2++, max2++)
                {
                    max3 = context.maskSize - 1;
                    for (int j3 = 0; j3 <= j3max; j3++, max3++)
                    {
                        tmp = 0;
                        for (int k1 = j1; k1 <= max1; k1++)
                            for (int k2 = j2; k2 <= max2; k2++)
                                for (int k3 = j3; k3 <= max3; k3++)
                                    tmp += context.hspdr[k1, k2, k3];

                        if (tmp > max)
                        {
                            max = tmp;
                            max_y = j1;
                            max_x = j2;
                            max_r = j3;
                        }
                    }
                }
            }
            int centerOffset = (context.maskSize - 1) / 2;
            max_x += centerOffset;
            max_y += centerOffset;
            max_r += centerOffset;
            context.result = new Circle(max_x, max_y, max_r);
            context.sum = max;
            return context;
        }

        private int xDiv = 2;
        private int yDiv = 2;

        public Circle Conv(double[,,] hspdr, int y, int x, int r, int masksize)
        {
            int j1max = y - masksize;
            int j2max = x - masksize;
            int j3max = r - masksize;

            int yStep = j1max / yDiv;
            int xStep = j2max / xDiv;
            if (yStep <= 0 && xStep <= 0)
            {
                Context context = new Context()
                {
                    hspdr = hspdr,
                    yStart = 0,
                    yEnd = j1max,
                    xStart = 0,
                    xEnd = j2max,
                    r = j3max,
                    maskSize = masksize,
                };
                Conv(context);
                return context.result;
            }
            Task<Context>[] tasks;
            if (yStep <= 0)
            {
                List<int> indices = new List<int>();
                indices.Add(0);
                int last = xStep;
                for (int i = 1; i < xDiv; i++)
                {
                    indices.Add(last);
                    indices.Add(last + 1);
                    last += xStep;
                }
                indices.Add(j2max);
                tasks = new Task<Context>[xDiv];
                for (int i = 0; i < xDiv; i++)
                {
                    int xStart = indices[i * 2];
                    int xEnd = indices[i * 2 + 1];
                    Task<Context> t = new Task<Context>(Conv, new Context()
                    {
                        hspdr = hspdr,
                        yStart = 0,
                        yEnd = j1max,
                        xStart = xStart,
                        xEnd = xEnd,
                        r = j3max,
                        maskSize = masksize,
                    });
                    t.Start();
                    tasks[i] = t;
                }
            }
            else if (xStep <= 0)
            {
                List<int> indices = new List<int>();
                indices.Add(0);
                int last = yStep;
                for (int i = 1; i < yDiv; i++)
                {
                    indices.Add(last);
                    indices.Add(last + 1);
                    last += yStep;
                }
                indices.Add(j1max);
                tasks = new Task<Context>[yDiv];
                for (int i = 0; i < yDiv; i++)
                {
                    int yStart = indices[i * 2];
                    int yEnd = indices[i * 2 + 1];
                    Task<Context> t = new Task<Context>(Conv, new Context()
                    {
                        hspdr = hspdr,
                        yStart = yStart,
                        yEnd = yEnd,
                        xStart = 0,
                        xEnd = j2max,
                        r = j3max,
                        maskSize = masksize,
                    });
                    t.Start();
                    tasks[i] = t;
                }
            }
            else
            {
                List<int> yIndices = new List<int>();
                yIndices.Add(0);
                int last = yStep;
                for (int i = 1; i < yDiv; i++)
                {
                    yIndices.Add(last);
                    yIndices.Add(last + 1);
                    last += yStep;
                }
                yIndices.Add(j1max);
                List<int> xIndices = new List<int>();
                xIndices.Add(0);
                last = xStep;
                for (int i = 1; i < xDiv; i++)
                {
                    xIndices.Add(last);
                    xIndices.Add(last + 1);
                    last += xStep;
                }
                xIndices.Add(j2max);
                tasks = new Task<Context>[yDiv * xDiv];
                int index = 0;
                for (int i = 0; i < yDiv; i++)
                {
                    int yStart = yIndices[i * 2];
                    int yEnd = yIndices[i * 2 + 1];
                    for (int j = 0; j < xDiv; j++)
                    {
                        int xStart = xIndices[j * 2];
                        int xEnd = xIndices[j * 2 + 1];
                        Task<Context> t = new Task<Context>(Conv, new Context()
                        {
                            hspdr = hspdr,
                            yStart = yStart,
                            yEnd = yEnd,
                            xStart = xStart,
                            xEnd = xEnd,
                            r = j3max,
                            maskSize = masksize,
                        });
                        t.Start();
                        tasks[index] = t;
                        index++;
                    }

                }
            }


            Context[] contexts = Task<Context>.WhenAll<Context>(tasks).GetAwaiter().GetResult();
            double max = 0;
            Circle c = default;
            foreach (Context context in contexts)
            {
                if (context.sum > max)
                {
                    c = context.result;
                    max = context.sum;
                }
            }
            return c;
        }
    }
}
