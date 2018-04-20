using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.NAudio
{
    public sealed class NAudioGetWaveform : IGetWaveform
    {
        private readonly IPeakProvider _peakProvider;
        private readonly IWaveProviderFactory _waveProviderFactory;

        public NAudioGetWaveform(IWaveProviderFactory waveProviderFactory = null, IPeakProvider peakProvider = null)
        {
            _waveProviderFactory = waveProviderFactory ?? new WaveProviderFactory();
            _peakProvider = peakProvider ?? new PeakProvider();
        }

        public WaveformData For(Uri source)
        {
            var audioStream = _waveProviderFactory.Create(source);
            using (audioStream as IDisposable)
                return Generate(audioStream);
        }

        private WaveformData Generate(IWaveProviderEx audioStream)
        {
            var position = audioStream.Position;
            var volume = audioStream.Volume;
            var pan = audioStream.Pan;
            audioStream.Position = 0;
            audioStream.Volume = 1f;
            audioStream.Pan = 0;

            try
            {
                var stopWatch = Stopwatch.StartNew();

                var peaks = new List<PeakInfo>();
                var buffer = new byte[audioStream.WaveFormat.AverageBytesPerSecond];

                int bytesRead;
                do
                {
                    bytesRead = audioStream.Read(buffer, 0, buffer.Length);
                    var data = bytesRead == buffer.Length ? buffer : buffer.Take(bytesRead).ToArray();
                    var p = _peakProvider.Sample(audioStream.WaveFormat, data);
                    peaks.AddRange(p);
                } while (bytesRead > 0 && audioStream.Position < audioStream.Length);

                var elapsed = stopWatch.Elapsed;
                var mibPerSecond = audioStream.Length / elapsed.TotalSeconds / 1024 / 1024;
#if DEBUG
                Trace.WriteLine($"Sampled '{audioStream.TotalTime}' of wave data in '{elapsed}' ({mibPerSecond:F1} MiB/sec).");
#endif
                return new WaveformData(audioStream.TotalTime, peaks);
            }
            finally
            {
                audioStream.Position = position;
                audioStream.Volume = volume;
                audioStream.Pan = pan;
            }
        }
    }
}
