using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NWaveform.Model;

namespace NWaveform.NAudio
{
    public class PeakProvider : IPeakProvider
    {
        public int PeaksPerSecond { get; set; } = 10;

        public PeakInfo[] Sample(WaveFormat waveFormat, byte[] data)
        {
            var sampleProvider = GetSampleProvider(waveFormat, data);

            var numSamples = waveFormat.SampleRate / PeaksPerSecond;
            var samples = new float[numSamples];

            var peaks = new List<PeakInfo>();
            var samplesRead = sampleProvider.Read(samples, 0, samples.Length);
            while (samplesRead > 0)
            {
                var peak = Filter(waveFormat, samples);
                peaks.Add(peak);
                samplesRead = sampleProvider.Read(samples, 0, samples.Length);
            }

            return peaks.ToArray();
        }

        private static PeakInfo Filter(WaveFormat waveFormat, float[] samples)
        {
            if (waveFormat.Channels != 2)
                return new PeakInfo(samples.Min(), samples.Max());

            // stereo
            var leftSamples = samples.Where((x, i) => i % 2 == 0).ToArray();
            var rightSamples = samples.Where((x, i) => i % 2 == 1).ToArray();

            return new PeakInfo(-rightSamples.Max(), leftSamples.Max());
        }

        private static ISampleProvider GetSampleProvider(WaveFormat waveFormat, byte[] data)
        {
            var waveProvider = new BufferedWaveProvider(waveFormat)
            {
                BufferLength = data.Length,
                ReadFully = false
            };
            waveProvider.ClearBuffer();
            waveProvider.AddSamples(data, 0, data.Length);
            var sampleProvider = waveProvider.ToSampleProvider();
            return sampleProvider;
        }
    }
}