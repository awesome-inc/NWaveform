using System;
using NAudio.CoreAudioApi;
using NEdifis.Attributes;

namespace NWaveform.Vlc
{
    [ExcludeFromConventions("not our code, copied from Internet: https://gist.github.com/mkoertgen/c4e1f788e793e77059b9")]
    public static class CoreAudio
    {
        public static AudioEndpointVolume GetDefaultVolumeEndpoint()
        {
            return new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).AudioEndpointVolume;
        }

        // left=0, right=1
        private const int LeftChan = 0;
        private const int RightChan = 1;

        /// <summary>
        /// Gets the ratio of volume across the left and right speakers in a range between -1 (left) and 1 (right). The default is 0 (center).
        /// </summary>
        /// <param name="volume">The audio volume endpoint</param>
        /// <returns></returns>
        public static float GetBalance(this AudioEndpointVolume volume)
        {
            VerifyChannels(volume);
            var masterVol = Math.Max(1e-6f, volume.MasterVolumeLevelScalar);
            var leftVol = volume.Channels[LeftChan].VolumeLevelScalar;
            var rightVol = volume.Channels[RightChan].VolumeLevelScalar;
            var balance = (rightVol - leftVol) / masterVol;
            return balance;
        }

        public static void SetBalance(this AudioEndpointVolume volume, float balance)
        {
            var safeBalance = Math.Max(-1, Math.Min(1, balance));
            var masterVol = volume.MasterVolumeLevelScalar;
            var rightVol = 1f + Math.Min(0f, safeBalance);
            var leftVol = 1f - Math.Max(0f, safeBalance);
            volume.Channels[LeftChan].VolumeLevelScalar = leftVol * masterVol;
            volume.Channels[RightChan].VolumeLevelScalar = rightVol * masterVol;
        }

        private static void VerifyChannels(AudioEndpointVolume volume)
        {
            if (volume == null) throw new ArgumentNullException(nameof(volume));
            if (volume.Channels.Count < 2)
                throw new InvalidOperationException("The specified audio endpoint does not expose left/right volume channels");
        }
    }
}