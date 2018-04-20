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
        private readonly WaveStream _audioStream;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly Task _task;
        private readonly TimeSpan _timeShift;

        public EndlessFileLoopChannel(IEventAggregator events, Uri source, WaveStream audioStream, TimeSpan bufferSize)
            : base(events, source, audioStream.WaveFormat, bufferSize)
        {
            _audioStream = audioStream;
            _task = Task.Factory.StartNew(PublishFromStream);
        }

        public EndlessFileLoopChannel(IEventAggregator events, Uri source, string fileName, TimeSpan bufferSize)
            : this(events, source, new AudioFileReader(fileName), bufferSize)
        {
        }

        public EndlessFileLoopChannel(IEventAggregator events, Uri source, string fileName, TimeSpan bufferSize, TimeSpan timeShift) 
            : this(events, source, fileName, bufferSize)
        {
            _timeShift = timeShift;
        }

        private void PublishFromStream()
        {
            var bufsize = _audioStream.WaveFormat.AverageBytesPerSecond; // / 25;
            var buffer = new byte[bufsize];
            var loops = 0;

            var sw = Stopwatch.StartNew();
            while (!_tokenSource.IsCancellationRequested)
            {
                sw.Restart();

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

                var time = DateTime.UtcNow.AddSeconds(_timeShift.TotalSeconds);
                AddSamples(buffer, 0, bytesRead, time);
#if DEBUG
                Trace.WriteLine($"Buffered '{Source}' ({loops}, {timeAfterRead} / {BufferedStream.CurrentWriteTime})...");
#endif
                timeDelta -= sw.Elapsed;
                if (timeDelta.TotalSeconds > 0)
                    Thread.Sleep(timeDelta);
            }
        }

        public override void Dispose()
        {
            if (_task.Status == TaskStatus.Running)
            {
                _tokenSource.Cancel();
                _task.Wait(TimeSpan.FromSeconds(1));
                if (_task.Status < TaskStatus.RanToCompletion)
                    Trace.TraceWarning($"Aborted task of '{nameof(EndlessFileLoopChannel)}'.");
                else
                    _task.Dispose();
            }
            _tokenSource.Dispose();
            _audioStream?.Dispose();
            base.Dispose();
        }
    }
}
