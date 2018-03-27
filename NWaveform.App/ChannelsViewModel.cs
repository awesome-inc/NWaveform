using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using NAudio.Wave;

namespace NWaveform.App
{
    internal class ChannelsViewModel : Screen, IChannelsViewModel
    {
        private readonly IEventAggregator _events;
        private readonly IChannelMixer _mixer;
        private readonly IWavePlayer _player;
        private IChannelViewModel _selectedChannel;
        private readonly BindableCollection<IChannelViewModel> _channels = new BindableCollection<IChannelViewModel>();

        public ChannelsViewModel(IEventAggregator events, IEnumerable<IChannelViewModel> channels,
            IChannelMixer mixer, IWavePlayer player)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _mixer = mixer ?? throw new ArgumentNullException(nameof(mixer));
            _player = player ?? throw new ArgumentNullException(nameof(player));

            channels.ToList().ForEach(AddChannel);
            SelectedChannel = Channels.FirstOrDefault();

            _player.Init(_mixer.SampleProvider);
            Play();
        }

        public IEnumerable<IChannelViewModel> Channels => _channels;

        public IChannelViewModel SelectedChannel
        {
            get => _selectedChannel;
            set
            {
                if (Equals(value, _selectedChannel)) return;
                _selectedChannel = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsPlaying => _player.PlaybackState == PlaybackState.Playing;

        public void Play()
        {
            _channels.Apply(c => c.IsPlaying = true);
            _player.Play();
            NotifyOfPropertyChange(nameof(IsPlaying));
        }

        public void Pause()
        {
            _channels.Apply(c => c.IsPlaying = false);
            _player.Pause();
            NotifyOfPropertyChange(nameof(IsPlaying));
        }

        public void ActivateSelected()
        {
            _events.PublishOnUIThread(new ActivateChannel(SelectedChannel.Source));
        }

        private void AddChannel(IChannelViewModel channel)
        {
            _channels.Add(channel);
            channel.IsPlaying = IsPlaying;
            _mixer.Add(channel.MixerInput);
        }
    }
}
