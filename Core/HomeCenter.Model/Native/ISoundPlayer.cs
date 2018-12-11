using System;

namespace HomeCenter.Model.Devices
{
    public interface ISoundPlayer
    {
        Action SoundEnd { get; set; }

        void Pause();

        void Play(string sound);
    }
}