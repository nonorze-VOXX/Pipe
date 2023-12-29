namespace fft
{
    public interface ISoundTriggerable
    {
        void Trigger();
        void SoundUpdate();
        bool IsTrigger();
    }
}