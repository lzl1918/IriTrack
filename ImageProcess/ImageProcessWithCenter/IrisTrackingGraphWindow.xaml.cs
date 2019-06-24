#define CASS_NO_DEFAULT

using CVImageLib;
using Emgu.CV;
using ImageProcess.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using RECT = System.Drawing.Rectangle;
using CIRCLE = CVImageLib.Circle;
using CVImageLib.Helpers;
using System.ComponentModel;

namespace ImageProcess
{
    internal class RetrieveResult
    {
        public FaceRetrieveResult Face;
        public EyeRetrieveResult Eye;
        public PupilRetrieveResult Pupil;
    }

    public partial class IrisTrackingGraphWindow : Window
    {
#if CASS_NO_DEFAULT
        public const string FACE_FILE = "haarcascade_frontalface_alt2.xml";
        public const string EYE_FILE = "haarcascade_eye.xml";
#else
        public const string FACE_FILE = "haarcascade_frontalface_default.xml";
        public const string EYE_FILE = "haarcascade_eye.xml";
#endif

        private VideoCapture capture = null;
        private VideoWriter captureOutput = null;
        private Mat cameraFrame;
        private Mat grayFrame;

        private IDetector detector = null;
        private CamerPreviewWindow resultWindow;

        private System.Drawing.Size gaussianSize = new System.Drawing.Size(9, 9);

        public const int BUFFER_SIZE = 100;
        private RingBuffer<double> leftData = new RingBuffer<double>(BUFFER_SIZE);
        private RingBuffer<double> rightData = new RingBuffer<double>(BUFFER_SIZE);
        private RingBuffer<double> leftTopData = new RingBuffer<double>(BUFFER_SIZE);
        private RingBuffer<double> rightTopData = new RingBuffer<double>(BUFFER_SIZE);
        private RingBuffer<double> leftDistData = new RingBuffer<double>(BUFFER_SIZE);
        private RingBuffer<double> rightDistData = new RingBuffer<double>(BUFFER_SIZE);
        private RingBuffer<double> leftTopDistData = new RingBuffer<double>(BUFFER_SIZE);
        private RingBuffer<double> rightTopDistData = new RingBuffer<double>(BUFFER_SIZE);

        private const int CENTER_BUFFER_SIZE = 5;
        private RingBuffer<double> centerLeftData = new RingBuffer<double>(CENTER_BUFFER_SIZE);
        private RingBuffer<double> centerRightData = new RingBuffer<double>(CENTER_BUFFER_SIZE);
        private RingBuffer<double> centerLeftTopData = new RingBuffer<double>(CENTER_BUFFER_SIZE);
        private RingBuffer<double> centerRightTopData = new RingBuffer<double>(CENTER_BUFFER_SIZE);
        private double centerLeft = 0;
        private double centerRight = 0;
        private double centerLeftTop = 0;
        private double centerRightTop = 0;

        private byte[] grayBytes;
        private byte[] drawBytes;
        private int img_width;
        private int img_height;
        private int img_size;
        private bool isFirstFrame = true;
        private bool isReady = true;
        private object _locker = new object();
        private bool isStarted = false;
        private DateTime startTime;
        private DateTime frameReadTime;
        private double lastDiff = 0;
        private bool isCloseRequested = false;

        public IrisTrackingGraphWindow()
        {
            InitializeComponent();

            canv_lt_dist.DrawingMaximum = 0.1;
            canv_rt_dist.DrawingMaximum = 0.1;

            Closing += OnClosing;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            double screenX = Screen.PrimaryScreen.WorkingArea.Width / 2;
            double screenY = Screen.PrimaryScreen.WorkingArea.Height / 2;
            double offsetY = Screen.PrimaryScreen.Bounds.Y;

            Left = Math.Max((screenX - ActualWidth) / 2, 0);
            Top = 40;

            resultWindow = new CamerPreviewWindow();
            resultWindow.Show();
            resultWindow.Left = Left;
            resultWindow.Top = Top + ActualHeight + 10;

            detector = new CascadeDetector(FACE_FILE, EYE_FILE);
            cameraFrame = new Mat();
            grayFrame = new Mat();
            try
            {
                capture = new VideoCapture();
                if (App.VideoPath != null)
                {

                    captureOutput = new VideoWriter(
                        App.VideoPath,
                        App.FrameRate,
                        new System.Drawing.Size((int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth), (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight)),
                        true);
                }
                capture.FlipHorizontal = true;
                capture.ImageGrabbed += OnFrameCaptured;
                capture.Grab();
            }
            catch (Exception excpt)
            {
                System.Windows.MessageBox.Show(excpt.Message);
            }


            canv_left.DataBuffer = leftData;
            canv_right.DataBuffer = rightData;
            canv_lt.DataBuffer = leftTopData;
            canv_rt.DataBuffer = rightTopData;
            canv_lt_dist.DataBuffer = leftDistData;
            canv_rt_dist.DataBuffer = rightDistData;

            btn_start.Click += (_sender, _e) =>
            {
                string content = btn_start.Content as string;
                if (content == "开始")
                {
                    btn_start.IsEnabled = false;
                    isStarted = true;
                    startTime = DateTime.Now;
                    btn_start.Content = "停止";
                }
            };
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!isCloseRequested)
            {
                if (captureOutput != null)
                {
                    captureOutput.Dispose();
                    captureOutput = null;
                }
                isCloseRequested = true;
                e.Cancel = true;
            }
        }

        private void OnFrameCaptured(object sender, EventArgs e)
        {
            lock (_locker)
                if (!isReady)
                    return;

            if (isCloseRequested)
            {
                resultWindow.NotifyClose();
                capture.Stop();
                capture.Dispose();
                cameraFrame.Dispose();
                grayFrame.Dispose();
                detector.Dispose();
                this.Close();
                return;
            }

            isReady = false;
            capture.Retrieve(cameraFrame, 0);
            if (captureOutput != null && isStarted)
            {
                captureOutput.Write(cameraFrame);
            }
            frameReadTime = DateTime.Now;
            if (isFirstFrame)
            {
                isFirstFrame = false;
                OnFirstFrame(cameraFrame.Width, cameraFrame.Height);
            }
            CvInvoke.CvtColor(cameraFrame, grayFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

            if (App.ShouldEqualizeHist)
                CvInvoke.EqualizeHist(grayFrame, grayFrame);
            // CvInvoke.GaussianBlur(_grayFrame, _grayFrame, _gaussianSize, 0);

            FindFaceFeatures();
        }

        private void OnFirstFrame(int width, int height)
        {
            img_width = width;
            img_height = height;
            img_size = img_height * img_width;
            grayBytes = new byte[img_size];
            drawBytes = new byte[img_size];
        }

        // run in ui thread
        private void OnFeatureRead(EyeRetrieveResult eyeRetrieveResult, PupilRetrieveResult pupilRetrieveResult)
        {
            double diff = (frameReadTime - startTime).TotalMilliseconds;
            if (startTime.Ticks == 0)
                diff = 0;
            if (diff > 15000)
            {
                Close();
                return;
            }
            double fps = diff.Equals(lastDiff) ? 0 : 1000 / (diff - lastDiff);
            lastDiff = diff;
            resultWindow.UpdateFPS(fps);
            if (eyeRetrieveResult.HasLeftEye)
            {
                double x_rato = (double)pupilRetrieveResult.LeftPupil.X / img_width;
                leftData.Add(x_rato);
                double y_rato = 1 - (double)pupilRetrieveResult.LeftPupil.Y / img_height;
                leftTopData.Add(y_rato);
                OnLeftPupil(diff, x_rato, y_rato);
            }
            if (eyeRetrieveResult.HasRightEye)
            {
                double x_rato = (double)pupilRetrieveResult.RightPupil.X / img_width;
                rightData.Add(x_rato);
                double y_rato = 1 - (double)pupilRetrieveResult.RightPupil.Y / img_height;
                rightTopData.Add(y_rato);
                OnRightPupil(diff, x_rato, y_rato);
            }
        }

        private void OnLeftPupil(double diff, double x, double y)
        {
            if (isStarted)
            {
                double diffX = x - centerLeft;
                double diffY = y - centerLeftTop;
                double dist = Math.Sqrt(diffX * diffX + diffY * diffY);
                leftDistData.Add(dist);
                Console.WriteLine($"l, {diff}, {x}, {y}, {centerLeft}, {centerLeftTop}, {diffX}, {diffY}");
            }
            else
            {
                centerLeftData.Add(x);
                centerLeftTopData.Add(y);
                centerLeft = GetAverage(centerLeftData);
                centerLeftTop = GetAverage(centerLeftTopData);
            }
        }

        private void OnRightPupil(double diff, double x, double y)
        {
            if (isStarted)
            {
                double diffX = x - centerRight;
                double diffY = y - centerRightTop;
                double dist = Math.Sqrt(diffX * diffX + diffY * diffY);
                rightDistData.Add(dist);
                Console.WriteLine($"r, {diff}, {x}, {y}, {centerRight}, {centerRightTop}, {diffX}, {diffY}");
            }
            else
            {
                centerRightData.Add(x);
                centerRightTopData.Add(y);
                centerRight = GetAverage(centerRightData);
                centerRightTop = GetAverage(centerRightTopData);
            }
        }

        private void FindFaceFeatures()
        {
            Marshal.Copy(grayFrame.DataPointer, grayBytes, 0, img_size);
            FindFaceEyesIrisAsync().ContinueWith((tsk) =>
            {
                RetrieveResult retrieveResult = tsk.Result;
                if (retrieveResult == null)
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BitmapSource grayImage = BitmapSource.Create(
                            img_width, img_height,
                            96.0, 96.0, PixelFormats.Gray8,
                            new BitmapPalette(new Color[] { Color.FromRgb(0, 0, 0), Color.FromRgb(255, 255, 255) }),
                            grayBytes, img_width);
                        resultWindow.SetResult(grayImage, grayImage);
                        lock (_locker)
                        {
                            isReady = true;
                            if (!capture.Grab())
                                App.Current.Shutdown();
                        }
                    }), DispatcherPriority.Render);
                    return;
                }

                FaceRetrieveResult faceRetrieveResult = retrieveResult.Face;
                EyeRetrieveResult eyeRetrieveResult = retrieveResult.Eye;
                PupilRetrieveResult pupilRetrieveResult = retrieveResult.Pupil;
                Emgu.CV.Structure.MCvScalar whiteColor = new Emgu.CV.Structure.MCvScalar(255, 255, 255);
                if (faceRetrieveResult.HasFace)
                {
                    RECT face = faceRetrieveResult.Face;
                    CvInvoke.Rectangle(grayFrame, face, whiteColor, 2);
                    if (eyeRetrieveResult.HasLeftEye)
                    {
                        RECT leftEye = eyeRetrieveResult.LeftEye;
                        CIRCLE leftPupil = pupilRetrieveResult.LeftPupil;
                        CvInvoke.Rectangle(grayFrame, leftEye, whiteColor, 2);
                        CvInvoke.Circle(grayFrame, leftPupil.Center, Math.Max(2, leftPupil.Radius), whiteColor);
                    }
                    if (eyeRetrieveResult.HasRightEye)
                    {
                        RECT rightEye = eyeRetrieveResult.RightEye;
                        CIRCLE rightPupil = pupilRetrieveResult.RightPupil;
                        CvInvoke.Rectangle(grayFrame, rightEye, whiteColor);
                        CvInvoke.Circle(grayFrame, rightPupil.Center, Math.Max(2, rightPupil.Radius), whiteColor);
                    }
                }

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    OnFeatureRead(eyeRetrieveResult, pupilRetrieveResult);
                    Marshal.Copy(grayFrame.DataPointer, drawBytes, 0, img_size);
                    BitmapSource grayImage = BitmapSource.Create(
                        img_width, img_height,
                        96.0, 96.0, PixelFormats.Gray8,
                        new BitmapPalette(new Color[] { Color.FromRgb(0, 0, 0), Color.FromRgb(255, 255, 255) }),
                        grayBytes, img_width);
                    BitmapSource resultImage = BitmapSource.Create(
                        img_width, img_height,
                        96.0, 96.0, PixelFormats.Gray8,
                        new BitmapPalette(new Color[] { Color.FromRgb(0, 0, 0), Color.FromRgb(255, 255, 255) }),
                        drawBytes, img_width);
                    resultWindow.SetResult(grayImage, resultImage);

                    lock (_locker)
                    {
                        isReady = true;
                        if (!capture.Grab())
                            App.Current.Shutdown();
                    }
                }), DispatcherPriority.Render);
            });
        }

        private Task<RetrieveResult> FindFaceEyesIrisAsync()
        {
            return Task<RetrieveResult>.Run(() =>
           {
               detector.DetectFace(grayFrame, out FaceRetrieveResult face, out EyeRetrieveResult eye);
               if (!face.HasFace)
                   return null;
               bool hasLeftPupil = false;
               CIRCLE leftPupil = default;
               if (eye.HasLeftEye)
               {
                   RECT leftEye = eye.LeftEye;
                   int offsety = (int)(leftEye.Height * 0.2);
                   int offsetx = (int)(leftEye.Width * 0.1);
                   leftEye.Offset(offsetx, offsety);
                   leftEye.Height = leftEye.Height - offsety - offsety;

                   leftPupil = Iris.SearchInnerBoundary(grayBytes, img_width, img_height, leftEye.Left, leftEye.Top, leftEye.Right, leftEye.Bottom);
               }
               bool hasRightPupil = false;
               CIRCLE rightPupil = default;
               if (eye.HasRightEye)
               {
                   RECT rightEye = eye.RightEye;
                   int offsety = (int)(rightEye.Height * 0.2);
                   int offsetx = (int)(rightEye.Width * 0.1);
                   rightEye.Offset(-offsetx, offsety);
                   rightEye.Height = rightEye.Height - offsety - offsety;
                   rightPupil = Iris.SearchInnerBoundary(grayBytes, img_width, img_height, rightEye.Left, rightEye.Top, rightEye.Right, rightEye.Bottom);
               }
               return new RetrieveResult()
               {
                   Face = face,
                   Eye = eye,
                   Pupil = new PupilRetrieveResult(hasLeftPupil, leftPupil, hasRightPupil, rightPupil),
               };
           });
        }

        private double GetAverage(RingBuffer<double> buffer)
        {
            if (buffer.Count <= 0)
                return 0;
            return buffer.Average();
        }
    }
}
