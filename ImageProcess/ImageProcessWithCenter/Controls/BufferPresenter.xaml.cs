using ImageProcess.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageProcess.Controls
{
    /// <summary>
    /// Interaction logic for BufferPresenter.xaml
    /// </summary>
    public partial class BufferPresenter : UserControl
    {
        public Brush ThemeBrush
        {
            get { return (Brush)GetValue(ThemeBrushProperty); }
            set { SetValue(ThemeBrushProperty, value); }
        }
        public static readonly DependencyProperty ThemeBrushProperty = DependencyProperty.Register("ThemeBrush", typeof(Brush), typeof(BufferPresenter), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 0, 0)), new PropertyChangedCallback(ThemeBrushPropertyChanged)));
        private static void ThemeBrushPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            BufferPresenter sender = obj as BufferPresenter;
            sender.border.BorderBrush = sender.ThemeBrush;
            sender.DataBuffer_BufferChanged(sender, new EventArgs());
        }

        public RingBuffer<double> DataBuffer
        {
            get { return (RingBuffer<double>)GetValue(DataBufferProperty); }
            set { SetValue(DataBufferProperty, value); }
        }
        public static readonly DependencyProperty DataBufferProperty = DependencyProperty.Register("DataBuffer", typeof(RingBuffer<double>), typeof(BufferPresenter), new PropertyMetadata(null, new PropertyChangedCallback(DataBufferPropertyChanged)));
        private static void DataBufferPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            BufferPresenter sender = obj as BufferPresenter;
            RingBuffer<double> oldvalue = args.OldValue as RingBuffer<double>;
            if (oldvalue != null)
                oldvalue.BufferChanged -= sender.DataBuffer_BufferChanged;
            if (sender.DataBuffer != null)
                sender.DataBuffer.BufferChanged += sender.DataBuffer_BufferChanged;
        }

        public double DrawingMaximum { get => maximum; set => maximum = value; }
        private double maximum;

        public BufferPresenter()
        {
            this.maximum = 1;
            InitializeComponent();
        }
        public BufferPresenter(double maximum)
        {
            this.maximum = maximum;
            InitializeComponent();
        }
        private void DataBuffer_BufferChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DataBuffer == null)
                    return;

                lock (DataBuffer)
                {
                    double height = canvas.ActualHeight;
                    double width = canvas.ActualWidth;
                    int maxlen = DataBuffer.MaxLength;
                    double gap = width / (maxlen - 1);
                    int i = DataBuffer.MaxLength - DataBuffer.Count;
                    PointCollection pc = new PointCollection();

                    foreach (var val in DataBuffer)
                    {
                        pc.Add(new Point(i * gap, (maximum - val) * height));
                        i++;
                    }
                    Visual polyline = canvas.Polyline(pc, ThemeBrush, 1);
                    canvas.Clear();
                    canvas.AddVisual(polyline);
                }
            }));

        }
    }
}
