using System;

namespace HomeCenter.Core.Interface.Native
{
    public interface ISoundPlayer
    {
        Action SoundEnd { get; set; }
        void Pause();
        void Play(string sound);
    }
}
