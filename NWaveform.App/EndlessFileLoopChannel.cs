using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NAudio.Wave;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class EndlessFileLoopChannel : IStreamingAudioChannel, IDisposable
    {
        private readonly IEventAggregator _events;
        public string Name { get; }
        public IWaveProviderEx Stream => _waveProvider;

        private readonly WaveStream _audioStream;
        private readonly BufferedWaveStream _bufferedStream;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly Task _task;
        private readonly WaveProviderEx _waveProvider;

        public EndlessFileLoopChannel(IEventAggregator events, string name, string fileName, TimeSpan bufferSize)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            _events = events;

            Name = name;
            _audioStream = new AudioFileReader(fileName);
            _bufferedStream = new BufferedWaveStream(_audioStream.WaveFormat, bufferSize)
            {
                DiscardOnBufferOverflow = true,
                ReadFully = true,
            };
            _waveProvider = new WaveProviderEx(_bufferedStream) {Closeable = false};

            _task = Task.Factory.StartNew(PublishFromStream);
        }

        private void PublishFromStream()
        {
            var buffer = new byte[_bufferedStream.WaveFormat.AverageBytesPerSecond * 1];

            var streamTime = TimeSpan.Zero;
            var loops = 0;

            while (!_tokenSource.IsCancellationRequested)
            {
                var timeBeforeRead = _audioStream.CurrentTime;
                var bytesRead = _audioStream.Read(buffer, 0, buffer.Length);
                var timeAfterRead = _audioStream.CurrentTime;
                var timeDelta = timeAfterRead - timeBeforeRead;

                if (bytesRead < buffer.Length)
                {
                    // if we have reached the end, reset stream to start
                    _audioStream.CurrentTime = TimeSpan.Zero;
                    loops++;
                    if (bytesRead == 0) continue;
                }

                _bufferedStream.AddSamples(buffer, 0, bytesRead);

                _events.PublishOnCurrentThread(new SamplesReceivedEvent(Name, streamTime, _bufferedStream.WaveFormat, buffer, bytesRead));
                Trace.WriteLine($"Buffered '{Name}' ({loops}, {timeAfterRead} / {streamTime})...");
                streamTime += timeDelta;

                Thread.Sleep(timeDelta);
            }
        }

        public void Dispose()
        {
            if (_task.Status == TaskStatus.Running)
            {
                _tokenSource.Cancel();
                _task.Wait();
            }
            _tokenSource.Dispose();
            _waveProvider?.ExplicitClose();
            _bufferedStream?.Dispose();
            _audioStream?.Dispose();
        }
    }
}