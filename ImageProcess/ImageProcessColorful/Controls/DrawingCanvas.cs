using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageProcess.Controls
{
    public class DrawingCanvas : Canvas
    {
        private List<Visual> visuals = new List<Visual>();

        protected override int VisualChildrenCount
        {
            get
            {
                return visuals.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        public void AddVisual(Visual visual)
        {
            visuals.Add(visual);

            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }
        public void RemoveVisual(Visual visual)
        {
            visuals.Remove(visual);

            base.RemoveVisualChild(visual);
            base.RemoveLogicalChild(visual);
        }

        public DrawingVisual GetVisual(Point point)
        {
            HitTestResult hitresult = VisualTreeHelper.HitTest(this, point);
            return hitresult.VisualHit as DrawingVisual;
        }
        public Visual Polyline(PointCollection points, Brush color, double thickness)
        {
            DrawingVisual visual = new DrawingVisual();
            DrawingContext dc = visual.RenderOpen();
            Pen pen = new Pen(color, thickness);
            pen.Freeze();
            for (int i = 0; i < points.Count - 1; i++)
                dc.DrawLine(pen, points[i], points[i + 1]);
            dc.Close();

            return visual;
        }
        public void Clear()
        {
            while (visuals.Count > 0)
            {
                RemoveVisual(visuals[0]);
            }
        }
    }
}
