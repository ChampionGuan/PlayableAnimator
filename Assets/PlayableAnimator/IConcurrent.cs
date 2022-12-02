namespace X3.PlayableAnimator
{
    public interface IConcurrent
    {
        void SetWeight(float weight);
        void SetTime(double time);
        void OnEnter();
        void OnExit();
        void OnPrepExit();
        void OnPrepEnter();
        void OnDestroy();
    }
}