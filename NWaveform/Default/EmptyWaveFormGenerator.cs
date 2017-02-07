using System;
using NWaveform.Events;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.Default
{
    public class EmptyWaveFormGenerator : IWaveFormGenerator
    {
        public WaveformData Generate(
            string source, Action<StreamVolumeEventArgs> onProgress = default(Action<StreamVolumeEventArgs>),
            int sampleRate = 20, int maxNumSamples = -1)
        {
            return CreateEmpty();
        }

        public static WaveformData CreateEmpty(double duration = 0)
        {
            return new WaveformData
            {
                Duration = TimeSpan.FromSeconds(duration),
                Channels = new [] { new Channel() }
            };
        }
    }
}