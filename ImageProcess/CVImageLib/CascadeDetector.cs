using CVImageLib.Helpers;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CVImageLib
{
    public class CascadeDetector : IDetector
    {
        private EyeSearchRegionExtractor searchRegionExtractor;
        private CascadeClassifier faceClassifier = null;
        private CascadeClassifier eyeClassifier = null;

        private double faceScaleFactor = 1.1;
        private int faceMinNeighbors = 20;
        private Size faceMinSize = new Size(20, 20);

        private double eyeScaleFactor = 1.1;
        private int eyeMinNeighbors = 20;
        private Size eyeMinSize = new Size(20, 20);

        public CascadeDetector(string faceFile, string eyeFile)
        {
            searchRegionExtractor = new EyeSearchRegionExtractor();
            faceClassifier = new CascadeClassifier(faceFile);
            eyeClassifier = new CascadeClassifier(eyeFile);
        }
        ~CascadeDetector()
        {
            if (disposed == false)
                Dispose();
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (disposed == true)
                return;

            disposed = true;
            faceClassifier.Dispose();
            eyeClassifier.Dispose();
        }

        public void DetectFace(Mat mat, out FaceRetrieveResult face, out EyeRetrieveResult eye)
        {
            Rectangle[] faceDetected = faceClassifier.DetectMultiScale(mat, faceScaleFactor, faceMinNeighbors, faceMinSize);
            if (faceDetected.Length <= 0)
            {
                face = FaceRetrieveResult.EmptyResult;
                eye = EyeRetrieveResult.EmptyResult;
                return;
            }

            Rectangle largestFace = RectangleHelper.GetMaximumRectangle(faceDetected);
            face = new FaceRetrieveResult(true, largestFace);
            searchRegionExtractor.GetSearchRegion(largestFace, out Rectangle leftSearchRegion, out Rectangle rightSearchRegion);
            leftSearchRegion.Offset(largestFace.X, largestFace.Y);
            rightSearchRegion.Offset(largestFace.X, largestFace.Y);
            bool hasLeftEye = false;
            Rectangle leftEyeRegion = default;
            using (Mat eyeSearchRegion = new Mat(mat, leftSearchRegion))
            {
                Rectangle[] eyesDetected = eyeClassifier.DetectMultiScale(eyeSearchRegion, eyeScaleFactor, eyeMinNeighbors, eyeMinSize);
                if (eyesDetected.Length > 0)
                {
                    hasLeftEye = true;
                    leftEyeRegion = RectangleHelper.GetMaximumRectangle(eyesDetected);
                    leftEyeRegion.Offset(leftSearchRegion.X, leftSearchRegion.Y);
                }
            }
            bool hasRightEye = false;
            Rectangle rightEyeRegion = default;
            using (Mat eyeSearchRegion = new Mat(mat, rightSearchRegion))
            {
                Rectangle[] eyesDetected = eyeClassifier.DetectMultiScale(eyeSearchRegion, eyeScaleFactor, eyeMinNeighbors, eyeMinSize);
                if (eyesDetected.Length > 0)
                {
                    hasRightEye = true;
                    rightEyeRegion = RectangleHelper.GetMaximumRectangle(eyesDetected);
                    rightEyeRegion.Offset(rightSearchRegion.X, rightSearchRegion.Y);
                }
            }
            eye = new EyeRetrieveResult(hasLeftEye, leftEyeRegion, hasRightEye, rightEyeRegion);
        }

        public FaceRetrieveResult FindFace(Mat image)
        {
            int size = Math.Min(image.Height, image.Width) / 8;
            Rectangle[] faces = faceClassifier.DetectMultiScale(image, 1.15, 7, new Size(size, size));
            if (faces.Length <= 0)
                return FaceRetrieveResult.EmptyResult;
            return new FaceRetrieveResult(true, RectangleHelper.GetMaximumRectangle(faces));
        }

        public EyeRetrieveResult FindEyes(Mat image, in Rectangle face)
        {
            searchRegionExtractor.GetSearchRegion(face, out Rectangle leftSearchRegion, out Rectangle rightSearchRegion);
            leftSearchRegion.Offset(face.X, face.Y);
            rightSearchRegion.Offset(face.X, face.Y);
            bool hasLeftEye = false;
            Rectangle leftEyeRegion = default;
            using (Mat eyeSearchRegion = new Mat(image, leftSearchRegion))
            {
                Rectangle[] eyesDetected = eyeClassifier.DetectMultiScale(eyeSearchRegion, eyeScaleFactor, eyeMinNeighbors, eyeMinSize);
                if (eyesDetected.Length > 0)
                {
                    hasLeftEye = true;
                    leftEyeRegion = RectangleHelper.GetMaximumRectangle(eyesDetected);
                }
            }
            bool hasRightEye = false;
            Rectangle rightEyeRegion = default;
            using (Mat eyeSearchRegion = new Mat(image, rightSearchRegion))
            {
                Rectangle[] eyesDetected = eyeClassifier.DetectMultiScale(eyeSearchRegion, eyeScaleFactor, eyeMinNeighbors, eyeMinSize);
                if (eyesDetected.Length > 0)
                {
                    hasRightEye = true;
                    rightEyeRegion = RectangleHelper.GetMaximumRectangle(eyesDetected);
                }
            }
            return new EyeRetrieveResult(hasLeftEye, leftEyeRegion, hasRightEye, rightEyeRegion);
        }
    }

}
