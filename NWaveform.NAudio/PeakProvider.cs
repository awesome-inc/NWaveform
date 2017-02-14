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

        public Func<float[], float> Filter { get; set; } = MagFilter;

        public static float MagFilter(float[] samples) { return samples.Select(Math.Abs).Max(); }
        public static float AvgFilter(float[] samples) { return samples.Average(); }
        public static float RmsFilter(float[] samples) { return (float)Math.Sqrt( samples.Select(x => x*x).Sum() / samples.Length ); }

        public PeakInfo[] Sample(WaveFormat waveFormat, byte[] data)
        {
            var sampleProvider = GetSampleProvider(waveFormat, data);

            var numSamples = waveFormat.SampleRate * waveFormat.Channels / PeaksPerSecond;
            var samples = new float[numSamples];

            var peaks = new List<PeakInfo>();
            var samplesRead = sampleProvider.Read(samples, 0, samples.Length);
            // only full buffers, so we get "exactly" #PeaksPerSecond peaks
            while (samplesRead == samples.Length)
            {
                var peak = GetPeaks(waveFormat, samples);
                peaks.Add(peak);
                samplesRead = sampleProvider.Read(samples, 0, samples.Length);
            }

            return peaks.ToArray();
        }

        private PeakInfo GetPeaks(WaveFormat waveFormat, float[] samples)
        {
            if (waveFormat.Channels != 2)
            {
                var peakValue = Filter(samples);
                return new PeakInfo(-peakValue, peakValue);
            }

            // stereo
            var leftSamples = samples.Where((x, i) => i % 2 == 0).ToArray();
            var rightSamples = samples.Where((x, i) => i % 2 == 1).ToArray();

            var leftPeak = Filter(leftSamples);
            var rightPeak = Filter(rightSamples);
            return new PeakInfo(-rightPeak, leftPeak);
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