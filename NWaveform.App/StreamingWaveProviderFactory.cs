using System;
using System.Collections.Generic;
using NWaveform.NAudio;

namespace NWaveform.App
{
    public class StreamingWaveProviderFactory : WaveProviderFactory
    {
        private readonly Dictionary<string, IStreamingAudioChannel> _channels = new Dictionary<string, IStreamingAudioChannel>();

        public StreamingWaveProviderFactory(IEnumerable<IStreamingAudioChannel> channels)
        {
            foreach(var channel in channels)
                _channels.Add(channel.Source.ToString(), channel);
        }

        public override IWaveProviderEx Create(Uri source)
        {
            return GetChannelFor(source)?.Stream ?? base.Create(source);
        }

        public override DateTimeOffset? For(Uri source)
        {
            return GetChannelFor(source)?.CreationTime ?? base.For(source);
        }

        private IStreamingAudioChannel GetChannelFor(Uri source)
        {
            IStreamingAudioChannel channel;
            _channels.TryGetValue(source.ToString(), out channel);
            return channel;
        }
    }
}