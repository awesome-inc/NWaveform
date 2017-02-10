using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NWaveform.Model;

namespace NWaveform.NAudio
{
    public class PeakProvider : IPeakProvider
    {
        public int PeaksPerSecond { get; set; } = 10;
        public Func<float[], PeakInfo> Filter { get; set; } = MaxFilter;

        public static PeakInfo MaxFilter(float[] samples)
        {
            return new PeakInfo(samples.Min(), samples.Max());
        }

        public PeakInfo[] Sample(WaveFormat waveFormat, byte[] data)
        {
            var waveProvider = new BufferedWaveProvider(waveFormat) { BufferLength = data.Length };
            waveProvider.AddSamples(data, 0, data.Length);

            var numSamples = waveFormat.SampleRate / PeaksPerSecond;
            var samples = new float[numSamples];
            //return Filter(waveFormat.SampleRate, samples);
            var sampleProvider = waveProvider.ToSampleProvider();

            var peaks = new List<PeakInfo>();
            var samplesRead = sampleProvider.Read(samples, 0, samples.Length);
            while (samplesRead > 0)
            {
                var peak = Filter(samples);
                peaks.Add(peak);
                samplesRead = sampleProvider.Read(samples, 0, samples.Length);
            }

            return peaks.ToArray();
        }
    }
}