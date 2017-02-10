using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class SamplesReceivedEvent
    {
        public string Source { get; }
        public TimeSpan Start { get; }
        public WaveFormat WaveFormat { get; }
        public byte[] Data { get; }

        public SamplesReceivedEvent(string source, TimeSpan start, WaveFormat waveFormat, byte[] data, int numBytes = 0)
        {
            Source = source;
            Start = start;
            WaveFormat = waveFormat;
            var n = numBytes != 0 ? numBytes : data.Length;
            Data = new byte[n];
            Array.Copy(data, Data, numBytes);
        }
    }
}