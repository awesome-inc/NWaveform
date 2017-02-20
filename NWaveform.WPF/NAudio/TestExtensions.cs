using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace NWaveform.NAudio
{
    internal static class TestExtensions
    {
        public static byte[] Generate(this WaveFormat waveFormat, TimeSpan interval,
            SignalGeneratorType type = SignalGeneratorType.Sin, double frequency = 1.0)
        {
            var sampleProvider = new SignalGenerator(waveFormat.SampleRate, waveFormat.Channels) { Frequency = frequency, Type = type };
            var waveProvider = new SampleToWaveProvider(sampleProvider);
            var numBytes = (int)(interval.TotalSeconds * waveFormat.AverageBytesPerSecond);
            var data = new byte[numBytes];
            waveProvider.Read(data, 0, data.Length);
            return data;
        }

        public static byte[] Generate(this TimeSpan interval, int rate = 8000, int channels = 1,
            SignalGeneratorType type = SignalGeneratorType.Sin, double frequency = 1.0)
        {
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(rate, channels);
            var sampleProvider = new SignalGenerator(rate, channels) { Frequency = frequency, Type = type };
            var waveProvider = new SampleToWaveProvider(sampleProvider);
            var numBytes = (int)(interval.TotalSeconds * waveFormat.AverageBytesPerSecond);
            var data = new byte[numBytes];
            waveProvider.Read(data, 0, data.Length);
            return data;
        }
    }
}