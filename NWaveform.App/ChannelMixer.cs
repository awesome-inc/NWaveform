using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace NWaveform.App
{
    internal class ChannelMixer : IChannelMixer
    {
        private readonly MixingSampleProvider _mixer;

        public ChannelMixer(MixingSampleProvider mixer = null)
        {
            _mixer = mixer ?? new MixingSampleProvider(MixerChannel.DefaultFormat);
        }

        public ISampleProvider SampleProvider => _mixer;

        public void Add(ISampleProvider channel)
        {
            _mixer.AddMixerInput(channel);
        }

        public void Remove(ISampleProvider channel)
        {
            _mixer.RemoveMixerInput(channel);
        }
    }
}
