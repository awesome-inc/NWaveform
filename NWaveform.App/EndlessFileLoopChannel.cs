using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class EndlessFileLoopChannel : IStreamingAudioChannel, IDisposable
    {
        public string Name { get; }
        public IWaveProviderEx Stream => _bufferedStream;

        private readonly WaveStream _audioStream;
        private readonly BufferedWaveStream _bufferedStream;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly Task _task;

        public EndlessFileLoopChannel(string name, string fileName, TimeSpan bufferSize)
        {
            Name = name;
            _audioStream = new AudioFileReader(fileName);
            _bufferedStream = new BufferedWaveStream(_audioStream.WaveFormat, bufferSize)
            {
                DiscardOnBufferOverflow = true,
                ReadFully = true,
            };

            _task = Task.Factory.StartNew(PublishFromStream);
        }

        private void PublishFromStream()
        {
            var buffer = new byte[_bufferedStream.WaveFormat.AverageBytesPerSecond * 1];
            var streamTimeStart = _audioStream.CurrentTime;
            var start = DateTime.UtcNow;
            var realTimeStart = start;
            var loops = 0;

            while (!_tokenSource.IsCancellationRequested)
            {
                var bytesRead = _audioStream.Read(buffer, 0, buffer.Length);
                if (bytesRead < buffer.Length)
                {
                    // if we have reached the end, reset stream to start
                    _audioStream.CurrentTime = TimeSpan.Zero;
                    streamTimeStart = _audioStream.CurrentTime;
                    realTimeStart = DateTime.UtcNow;
                    loops++;
                    if (bytesRead == 0) continue;
                }

                _bufferedStream.AddSamples(buffer, 0, bytesRead);


                var streamTimePassed = _audioStream.CurrentTime - streamTimeStart;
                var now = DateTime.UtcNow;
                var passed = now - start;
                var realTimePassed = now - realTimeStart;
                var timeDifference = Math.Max(0, (streamTimePassed - realTimePassed).TotalMilliseconds);

                Trace.WriteLine($"Buffered '{Name}' ({loops}, {passed}, {realTimePassed})...");

                // TODO: _eventAggregator.Publish(samples);

                Thread.Sleep((int) timeDifference);
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
            _audioStream?.Dispose();
            _bufferedStream?.Dispose();
        }
    }
}