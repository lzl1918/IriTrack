using System.Drawing;

namespace CVImageLib
{
    public class EyeRetrieveResult
    {
        public static EyeRetrieveResult EmptyResult { get; } = new EyeRetrieveResult(false, default, false, default);

        public EyeRetrieveResult(bool hasLeftEye, Rectangle leftEye, bool hasRightEye, Rectangle rightEye)
        {
            HasLeftEye = hasLeftEye;
            LeftEye = leftEye;
            HasRightEye = hasRightEye;
            RightEye = rightEye;
        }

        public bool HasLeftEye { get; }
        public Rectangle LeftEye { get; }
        public bool HasRightEye { get; }
        public Rectangle RightEye { get; }

    }

}
