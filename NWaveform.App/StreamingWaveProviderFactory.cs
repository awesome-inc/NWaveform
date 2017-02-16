using System;
using System.Collections.Generic;
using NWaveform.NAudio;

namespace NWaveform.App
{
    public class StreamingWaveProviderFactory : WaveProviderFactory
    {
        private readonly Dictionary<string, IWaveProviderEx> _channels = new Dictionary<string, IWaveProviderEx>();

        public StreamingWaveProviderFactory(IEnumerable<IStreamingAudioChannel> channels)
        {
            foreach(var channel in channels)
                _channels.Add(channel.Source.ToString(), channel.Stream);
        }

        public override IWaveProviderEx Create(Uri source)
        {
            IWaveProviderEx channel;
            if (_channels.TryGetValue(source.ToString(), out channel)) return channel;
            return base.Create(source);
        }
    }
}