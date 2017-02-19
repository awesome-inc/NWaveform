using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NAudio.Wave;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class EndlessFileLoopChannel : BufferedStreamingChannel
    {
        private readonly IEventAggregator _events;
        private readonly WaveStream _audioStream;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly Task _task;

        public EndlessFileLoopChannel(IEventAggregator events, Uri source, WaveStream audioStream, TimeSpan bufferSize)
            : base(source, audioStream.WaveFormat, bufferSize)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            _events = events;
            _audioStream = audioStream;
            _task = Task.Factory.StartNew(PublishFromStream);
        }

        public EndlessFileLoopChannel(IEventAggregator events, Uri source, string fileName, TimeSpan bufferSize)
            : this(events, source, new AudioFileReader(fileName), bufferSize)
        {
        }

        private void PublishFromStream()
        {
            var buffer = new byte[Stream.WaveFormat.AverageBytesPerSecond * 1];
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

                var streamTime = BufferedStream.CurrentWriteTime;

                AddSamples(streamTime, buffer, bytesRead);
                _events.PublishOnCurrentThread(new SamplesReceivedEvent(Source, streamTime, Stream.WaveFormat, buffer, bytesRead));
                Trace.WriteLine($"Buffered '{Source}' ({loops}, {timeAfterRead} / {BufferedStream.CurrentWriteTime})...");

                Thread.Sleep(timeDelta);
            }
        }

        public override void Dispose()
        {
            if (_task.Status == TaskStatus.Running)
            {
                _tokenSource.Cancel();
                _task.Wait();
            }
            _tokenSource.Dispose();

            _audioStream?.Dispose();

            base.Dispose();
        }
    }
}