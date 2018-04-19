using System;
using System.Collections.Generic;
using System.Linq;
using NWaveform.NAudio;

namespace NWaveform.App
{
    public class StreamingWaveProviderFactory : WaveProviderFactory
    {
        private readonly IChannelFactory _channelFactory;
        private readonly Dictionary<string, IStreamingAudioChannel> _channels = new Dictionary<string, IStreamingAudioChannel>();

        public StreamingWaveProviderFactory(IEnumerable<IStreamingAudioChannel> channels, 
            IChannelFactory channelFactory)
        {
            _channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
            channels.ToList().ForEach(AddChannel);
        }

        public override IWaveProviderEx Create(Uri source)
        {
            return GetChannelFor(source)?.Stream ?? base.Create(source);
        }

        public override DateTimeOffset? For(Uri source)
        {
            return GetChannelFor(source)?.StartTime ?? base.For(source);
        }

        private IStreamingAudioChannel GetChannelFor(Uri source)
        {
            if (_channels.TryGetValue(source.ToString(), out var channel)) return channel;
            channel = _channelFactory.Create(source);
            if (channel == null) return null;
            AddChannel(channel);
            return channel;
        }

        private void AddChannel(IStreamingAudioChannel channel)
        {
            _channels.Add(channel.Source.ToString(), channel);
        }
    }
}
