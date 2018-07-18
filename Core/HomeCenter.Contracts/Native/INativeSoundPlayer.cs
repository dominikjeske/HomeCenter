using System;

namespace HomeCenter.Core.Interface.Native
{
    public interface INativeSoundPlayer
    {
        Action SoundEnd { get; set; }
        void Pause();
        void Play(string sound);
    }
}
