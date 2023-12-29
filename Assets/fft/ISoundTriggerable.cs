using UnityEngine;

namespace fft
{
    public interface ISoundTriggerable
    {
        void Trigger(Color color);
        void SoundUpdate();
        bool IsTrigger();
    }
}