using System.Drawing;

namespace CVImageLib
{
    public class FaceRetrieveResult
    {
        public static FaceRetrieveResult EmptyResult { get; } = new FaceRetrieveResult(false, default);

        public FaceRetrieveResult(bool hasFace, Rectangle face)
        {
            HasFace = hasFace;
            Face = face;
        }

        public bool HasFace { get; }
        public Rectangle Face { get; }
    }

}
