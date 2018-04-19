using System;
using Caliburn.Micro;
using NAudio.Wave;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class Mp3StreamChannelFactory : IChannelFactory
    {
        private readonly IEventAggregator _events;
        private readonly TimeSpan _bufferSize;

        public Mp3StreamChannelFactory(IEventAggregator events, TimeSpan bufferSize)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _bufferSize = bufferSize;
        }

        public IStreamingAudioChannel Create(Uri source)
        {
            if (source.IsFile)
                return null;
            var waveFormat = GetWaveFormat(source);
            return new Mp3StreamChannel(_events, source, waveFormat, _bufferSize);
        }

        private static WaveFormat GetWaveFormat(Uri source)
        {
            using (var readerStream = new MediaFoundationReader(source.ToString()))
                return readerStream.WaveFormat;
        }
    }
}
