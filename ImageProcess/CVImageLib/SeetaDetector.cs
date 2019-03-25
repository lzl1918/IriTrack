//using Emgu.CV;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace CVImageLib
//{
//    public class SeetaDetector : IDetector
//    {
//        public const string DLL_NAME = "FaceDetection.dll";
//        public const CallingConvention CALLING = CallingConvention.Cdecl;
//        public const CharSet CHARSET = CharSet.Ansi;

//        [DllImport(DLL_NAME, CallingConvention = CALLING, CharSet = CHARSET)]
//        private extern static IntPtr SetupDetector(string modulePath, int minFaceSize, float scoreThresh, float pyramidScale, int windowStepX, int windowStepY);

//        [DllImport(DLL_NAME, CallingConvention = CALLING, CharSet = CHARSET)]
//        private extern static void FreeDetector(ref IntPtr detector);

//        [DllImport(DLL_NAME, CallingConvention = CALLING, CharSet = CHARSET)]
//        private extern static IntPtr DetectFace(IntPtr detector, IntPtr input);

//        [DllImport(DLL_NAME, CallingConvention = CALLING, CharSet = CHARSET)]
//        private extern static IntPtr FreeEnumerator(ref IntPtr enumerator);

//        [DllImport(DLL_NAME, CallingConvention = CALLING, CharSet = CHARSET)]
//        private extern static bool MoveNext(IntPtr enumerator);

//        [DllImport(DLL_NAME, CallingConvention = CALLING, CharSet = CHARSET)]
//        private extern static void Value(IntPtr enumerator, out int x, out int y, out int width, out int height);

//        [DllImport(DLL_NAME, CallingConvention = CALLING, CharSet = CHARSET)]
//        private extern static void Reset(IntPtr enumerator);


//        private IntPtr detector;
//        private CascadeClassifier eyeClassifier = null;
//        public SeetaDetector(string moduleFile, string eyeFile)
//        {
//            eyeClassifier = new CascadeClassifier(eyeFile);
//            detector = SetupDetector(moduleFile, 20, 2f, 0.8f, 4, 4);
//        }
//        ~SeetaDetector()
//        {
//            if (disposed == false)
//                Dispose();
//        }

//        private bool disposed = false;
//        public void Dispose()
//        {
//            if (disposed == true)
//                return;

//            disposed = true;
//            eyeClassifier.Dispose();
//            FreeDetector(ref detector);
//        }

//        public void DetectFace(Mat mat, out FaceRetrieveResult face, out EyeRetrieveResult eye)
//        {
//            int x, y, width, height;
//            int maxsize = 0;
//            int x0 = 0, y0 = 0, width0 = 0, height0 = 0;
//            IntPtr enumerator = DetectFace(detector, mat.Ptr);
//            List<Rectangle> faceDetected = new List<Rectangle>();
//            while (MoveNext(enumerator))
//            {
//                Value(enumerator, out x, out y, out width, out height);
//                int size = width * height;
//                if (size > maxsize)
//                {
//                    x0 = x;
//                    y0 = y;
//                    width0 = width;
//                    height0 = height;
//                    maxsize = size;
//                }
//            }
//            FreeEnumerator(ref enumerator);

//            if (maxsize <= 0)
//            {
//                face = FaceRetrieveResult.EmptyResult;
//                eye = EyeRetrieveResult.EmptyResult;
//                return;
//            }

//            Rectangle largestFace = new Rectangle(x0, y0, width0, height0);
//            face = new FaceRetrieveResult(true, largestFace);
//            Rectangle eyepredict = new Rectangle((int)(largestFace.Width * 0.10), (int)(largestFace.Height * 0.2), (int)(largestFace.Width * 0.4), (int)(largestFace.Height * 0.3));
//            eyepredict.Offset(largestFace.X, largestFace.Y);
//            using (Mat faceRegion = new Mat(mat, eyepredict))
//            {
//                Rectangle[] eyesDetected = eyeClassifier.DetectMultiScale(
//                    faceRegion,
//                    scaleFactor: 1.1,
//                    minNeighbors: 20,
//                    minSize: new Size(20, 20)
//                );
//                if (eyesDetected.Length > 0)
//                {
//                    Rectangle eyeRect = eyesDetected[0];
//                    eyeRect.Offset(eyepredict.X, eyepredict.Y);
//                    eyes.Add(eyeRect);
//                }
//            }

//            eyepredict = new Rectangle((int)(largestFace.Width * 0.5), (int)(largestFace.Height * 0.2), (int)(largestFace.Width * 0.4), (int)(largestFace.Height * 0.3));
//            eyepredict.Offset(largestFace.X, largestFace.Y);
//            using (Mat faceRegion = new Mat(mat, eyepredict))
//            {
//                Rectangle[] eyesDetected = eyeClassifier.DetectMultiScale(
//                    faceRegion,
//                    scaleFactor: 1.1,
//                    minNeighbors: 20,
//                    minSize: new Size(20, 20)
//                );

//                if (eyesDetected.Length > 0)
//                {
//                    Rectangle eyeRect = eyesDetected[0];
//                    eyeRect.Offset(eyepredict.X, eyepredict.Y);
//                    eyes.Add(eyeRect);
//                }
//            }
//        }

//        public (bool hasFace, Rectangle face) FindLargestFace(Mat mat)
//        {
//            int x, y, width, height;
//            int maxsize = 0;
//            int x0 = 0, y0 = 0, width0 = 0, height0 = 0;

//            IntPtr enumerator = DetectFace(detector, mat.Ptr);
//            if (enumerator.Equals(IntPtr.Zero))
//                return (false, default(Rectangle));
//            List<Rectangle> faceDetected = new List<Rectangle>();

//            while (MoveNext(enumerator))
//            {
//                Value(enumerator, out x, out y, out width, out height);
//                if (width * height > maxsize)
//                {
//                    x0 = x;
//                    y0 = y;
//                    width0 = width;
//                    height0 = height;
//                    maxsize = width * height;
//                }
//            }
//            FreeEnumerator(ref enumerator);

//            if (maxsize <= 0)
//                return (false, default(Rectangle));

//            return (true, new Rectangle(x0, y0, width0, height0));
//        }
//        public (bool hasLeft, Rectangle left, bool hasRight, Rectangle right) FindEyes(Mat image, Rectangle face)
//        {
//            int width = face.Width;
//            int height = face.Height;
//            int size = Math.Max(Math.Min(width, height) / 10, 5);
//            Size detectSize = new Size(size, size);

//            bool hasLeft = false;
//            Rectangle left = default(Rectangle);
//            {
//                int leftx = (int)(face.Left + width * 0.1);
//                int lefty = (int)(face.Top + height * 0.2);
//                int leftw = (int)(width * 0.5);
//                int lefth = (int)(height * 0.3);
//                using (Mat leftEye = new Mat(image, new Rectangle(leftx, lefty, leftw, lefth)))
//                {
//                    Rectangle[] rects = eyeClassifier.DetectMultiScale(leftEye, 1.2, 7, detectSize);
//                    if (rects.Length > 0)
//                    {
//                        hasLeft = true;
//                        left = rects[0];
//                        left.Offset(leftx, lefty);
//                    }
//                }
//            }
//            bool hasRight = false;
//            Rectangle right = default(Rectangle);
//            {
//                int rightx = (int)(face.Left + width * 0.4);
//                int righty = (int)(face.Top + height * 0.2);
//                int rightw = (int)(width * 0.5);
//                int righth = (int)(height * 0.3);
//                using (Mat rightEye = new Mat(image, new Rectangle(rightx, righty, rightw, righth)))
//                {
//                    Rectangle[] rects = eyeClassifier.DetectMultiScale(rightEye, 1.1, 10, detectSize);
//                    if (rects.Length > 0)
//                    {
//                        hasRight = true;
//                        right = rects[0];
//                        right.Offset(rightx, righty);
//                    }
//                }
//            }
//            return (hasLeft, left, hasRight, right);
//        }
//    }
//}
