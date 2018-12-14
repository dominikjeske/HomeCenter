using System;

namespace HomeCenter.Model.Contracts
{
    public interface ISoundPlayer
    {
        Action SoundEnd { get; set; }

        void Pause();

        void Play(string sound);
    }
}