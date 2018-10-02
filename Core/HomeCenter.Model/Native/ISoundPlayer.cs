using System;

namespace HomeCenter.Model.Native
{
    public interface ISoundPlayer
    {
        Action SoundEnd { get; set; }

        void Pause();

        void Play(string sound);
    }
}