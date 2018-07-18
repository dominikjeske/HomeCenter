using System;
using System.Runtime.InteropServices;
using Wirehome.WindowsService.Audio;
using Wirehome.WindowsService.Interop;

namespace Wirehome.WindowsService.Core
{
    public class AudioService : IAudioService
    {
        private readonly IMMDeviceEnumerator _deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();

        public AudioDeviceCollection GetAudioDevices(AudioDeviceKind kind = AudioDeviceKind.Playback, AudioDeviceState state = AudioDeviceState.Active)
        {
            int hr = _deviceEnumerator.EnumAudioEndpoints(kind, state, out IMMDeviceCollection underlyingCollection);
            if (hr == HResult.OK)  return new AudioDeviceCollection(underlyingCollection);

            throw Marshal.GetExceptionForHR(hr);
        }

        public void SetDefaultAudioDevice(AudioDevice device)
        {
            if (device == null)  throw new ArgumentNullException(nameof(device));

            SetDefaultAudioDevice(device, AudioDeviceRole.Multimedia);
            SetDefaultAudioDevice(device, AudioDeviceRole.Communications);
            SetDefaultAudioDevice(device, AudioDeviceRole.Console);
        }

        public void SetDefaultAudioDevice(AudioDevice device, AudioDeviceRole role = AudioDeviceRole.Multimedia)
        {
            if (device == null)  throw new ArgumentNullException(nameof(device));

            var config = new PolicyConfig();

            int hr;
            if (config is IPolicyConfig2 config2)
            {   // Windows 7 -> Windows 8.1
                hr = config2.SetDefaultEndpoint(device.Id, role);
            }
            else
            {   // Windows 10+
                hr = ((IPolicyConfig3)config).SetDefaultEndpoint(device.Id, role);
            }

            if (hr != HResult.OK)
                throw Marshal.GetExceptionForHR(hr);
        }

        public bool IsDefaultAudioDevice(AudioDevice device, AudioDeviceRole role = AudioDeviceRole.Multimedia)
        {
            if (device == null)  throw new ArgumentNullException(nameof(device));

            AudioDevice defaultDevice = GetDefaultAudioDevice(device.Kind, role);
            if (defaultDevice == null)  return false;

            return String.Equals(defaultDevice.Id, device.Id, StringComparison.OrdinalIgnoreCase);
        }

        public AudioDevice GetDefaultAudioDevice(AudioDeviceKind kind = AudioDeviceKind.Playback, AudioDeviceRole role = AudioDeviceRole.Multimedia)
        {
            int hr = _deviceEnumerator.GetDefaultAudioEndpoint(kind, role, out IMMDevice underlyingDevice);
            if (hr == HResult.OK)  return new AudioDevice(underlyingDevice);

            if (hr == HResult.NotFound || hr == HResult.FileNotFound) return null;

            throw Marshal.GetExceptionForHR(hr);
        }

        public AudioDevice GetDevice(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            int hr = _deviceEnumerator.GetDevice(id, out IMMDevice underlyingDevice);
            if (hr == HResult.OK) return new AudioDevice(underlyingDevice);

            if (hr == HResult.NotFound) return null;

            throw Marshal.GetExceptionForHR(hr);
        }
 
        public float GetMasterVolume()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null) return -1;

                masterVol.GetMasterVolumeLevelScalar(out float volumeLevel);
                return volumeLevel * 100;
            }
            finally
            {
                if (masterVol != null)  Marshal.ReleaseComObject(masterVol);
            }
        }

        public bool GetMasterVolumeMute()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null) return false;

                masterVol.GetMute(out bool isMuted);
                return isMuted;
            }
            finally
            {
                if (masterVol != null) Marshal.ReleaseComObject(masterVol);
            }
        }

        public void SetMasterVolume(float newLevel)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null) return;

                masterVol.SetMasterVolumeLevelScalar(newLevel / 100, Guid.Empty);
            }
            finally
            {
                if (masterVol != null) Marshal.ReleaseComObject(masterVol);
            }
        }

        public float StepMasterVolume(float stepAmount)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null) return -1;

                float stepAmountScaled = stepAmount / 100;

                masterVol.GetMasterVolumeLevelScalar(out float volumeLevel);

                float newLevel = volumeLevel + stepAmountScaled;
                newLevel = Math.Min(1, newLevel);
                newLevel = Math.Max(0, newLevel);

                masterVol.SetMasterVolumeLevelScalar(newLevel, Guid.Empty);

                return newLevel * 100;
            }
            finally
            {
                if (masterVol != null) Marshal.ReleaseComObject(masterVol);
            }
        }

        public void SetMasterVolumeMute(bool isMuted)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null) return;

                masterVol.SetMute(isMuted, Guid.Empty);
            }
            finally
            {
                if (masterVol != null) Marshal.ReleaseComObject(masterVol);
            }
        }

        public bool ToggleMasterVolumeMute()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null) return false;

                masterVol.GetMute(out bool isMuted);
                masterVol.SetMute(!isMuted, Guid.Empty);

                return !isMuted;
            }
            finally
            {
                if (masterVol != null) Marshal.ReleaseComObject(masterVol);
            }
        }

        private IAudioEndpointVolume GetMasterVolumeObject()
        {
            IMMDeviceEnumerator deviceEnumerator = null;
            IMMDevice speakers = null;
            try
            {
                deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                deviceEnumerator.GetDefaultAudioEndpoint(AudioDeviceKind.Playback, AudioDeviceRole.Multimedia, out speakers);

                var IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
                speakers.Activate(ref IID_IAudioEndpointVolume, 0, IntPtr.Zero, out object o);
                return (IAudioEndpointVolume)o;
            }
            finally
            {
                if (speakers != null) Marshal.ReleaseComObject(speakers);
                if (deviceEnumerator != null) Marshal.ReleaseComObject(deviceEnumerator);
            }
        }
        
        public float? GetApplicationVolume(int pid)
        {
            var volume = GetVolumeObject(pid);
            if (volume == null) return null;

            volume.GetMasterVolume(out float level);
            Marshal.ReleaseComObject(volume);
            return level * 100;
        }

        public bool? GetApplicationMute(int pid)
        {
            var volume = GetVolumeObject(pid);
            if (volume == null) return null;

            volume.GetMute(out bool mute);
            Marshal.ReleaseComObject(volume);
            return mute;
        }

        public void SetApplicationVolume(int pid, float level)
        {
            var volume = GetVolumeObject(pid);
            if (volume == null) return;

            Guid guid = Guid.Empty;
            volume.SetMasterVolume(level / 100, ref guid);
            Marshal.ReleaseComObject(volume);
        }

        public void SetApplicationMute(int pid, bool mute)
        {
            ISimpleAudioVolume volume = GetVolumeObject(pid);
            if (volume == null)  return;

            Guid guid = Guid.Empty;
            volume.SetMute(mute, ref guid);
            Marshal.ReleaseComObject(volume);
        }

        private ISimpleAudioVolume GetVolumeObject(int pid)
        {
            IMMDeviceEnumerator deviceEnumerator = null;
            IAudioSessionEnumerator sessionEnumerator = null;
            IAudioSessionManager2 mgr = null;
            IMMDevice speakers = null;
            try
            {
                // get the speakers (1st render + multimedia) device
                deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                deviceEnumerator.GetDefaultAudioEndpoint(AudioDeviceKind.Playback, AudioDeviceRole.Multimedia, out speakers);

                // activate the session manager. we need the enumerator
                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out object o);
                mgr = (IAudioSessionManager2)o;

                // enumerate sessions for on this device
                mgr.GetSessionEnumerator(out sessionEnumerator);
                sessionEnumerator.GetCount(out int count);

                // search for an audio session with the required process-id
                ISimpleAudioVolume volumeControl = null;
                for (int i = 0; i < count; ++i)
                {
                    IAudioSessionControl2 ctl = null;
                    try
                    {
                        sessionEnumerator.GetSession(i, out ctl);

                        // NOTE: we could also use the app name from ctl.GetDisplayName()
                        ctl.GetProcessId(out int cpid);

                        if (cpid == pid)
                        {
                            volumeControl = ctl as ISimpleAudioVolume;
                            break;
                        }
                    }
                    finally
                    {
                        if (ctl != null) Marshal.ReleaseComObject(ctl);
                    }
                }

                return volumeControl;
            }
            finally
            {
                if (sessionEnumerator != null) Marshal.ReleaseComObject(sessionEnumerator);
                if (mgr != null) Marshal.ReleaseComObject(mgr);
                if (speakers != null) Marshal.ReleaseComObject(speakers);
                if (deviceEnumerator != null) Marshal.ReleaseComObject(deviceEnumerator);
            }
        }
    }
}