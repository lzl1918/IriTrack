namespace CVImageLib
{
    public class PupilRetrieveResult
    {
        public static PupilRetrieveResult EmptyResult { get; } = new PupilRetrieveResult(false, default, false, default);

        public PupilRetrieveResult(bool hasLeftPupil, Circle leftPupil, bool hasRightPupil, Circle rightPupil)
        {
            HasLeftPupil = hasLeftPupil;
            LeftPupil = leftPupil;
            HasRightPupil = hasRightPupil;
            RightPupil = rightPupil;
        }

        public bool HasLeftPupil { get; }
        public Circle LeftPupil { get; }
        public bool HasRightPupil { get; }
        public Circle RightPupil { get; }
    }

}
