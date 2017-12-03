using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NAudio.Wave;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class Mp3StreamChannel : BufferedStreamingChannel
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly Task _task;
        private readonly byte[] _buffer;
        private readonly Stream _stream;
        private IMp3FrameDecompressor _decompressor;

        public Mp3StreamChannel(IEventAggregator events, Uri source, WaveFormat waveFormat, TimeSpan bufferSize) 
            : base(events, source, waveFormat, bufferSize)
        {
            _buffer = new byte[BufferedStream.WaveFormat.AverageBytesPerSecond];
            _stream = CreateStream(Source);
            _task = Task.Factory.StartNew(PublishFromStream);
        }

        public override void Dispose()
        {
            if (_task.Status == TaskStatus.Running)
            {
                _tokenSource.Cancel();
                _task.Wait(TimeSpan.FromSeconds(1));
                if (_task.Status < TaskStatus.RanToCompletion)
                    Trace.TraceWarning($"Aborted task of '{nameof(Mp3StreamChannel)}'.");
                else
                    _task.Dispose();
            }
            _tokenSource.Dispose();


            _decompressor?.Dispose();
            _stream.Dispose();

            base.Dispose();
        }

        private void PublishFromStream()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                var frame = Mp3Frame.LoadFromStream(_stream);
                if (_decompressor == null)
                    _decompressor = CreateFrameDecompressor(frame);
                var bytesRead = _decompressor.DecompressFrame(frame, _buffer, 0);
                if (bytesRead == 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    continue;
                }
                AddSamples(_buffer, 0, bytesRead);
                Trace.WriteLine($"Buffered '{Source}' ({BufferedStream.CurrentWriteTime})...");
            }
            Trace.WriteLine($"Stopped reading from '{Source}' ({BufferedStream.CurrentWriteTime}).");
        }

        private static Stream CreateStream(Uri source)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(source);
            var response = (HttpWebResponse)webRequest.GetResponse();
            return new ReadFullyStream(response.GetResponseStream());
        }

        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }
    }
}