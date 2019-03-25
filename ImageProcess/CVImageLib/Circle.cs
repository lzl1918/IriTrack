using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVImageLib
{
    public struct Circle
    {
        private int x;
        private int y;
        private int radius;
        private Point center;

        public int X => x;
        public int Y => y;
        public int Radius => radius;
        public Point Center => center;

        public Circle(int x, int y, int radius)
        {
            this.x = x;
            this.y = y;
            this.radius = radius;
            this.center = new Point(x, y);
        }
    }
}
