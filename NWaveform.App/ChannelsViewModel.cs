using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace NWaveform.App
{
    internal class ChannelsViewModel : Screen, IChannelsViewModel
    {
        private IChannelViewModel _selectedChannel;
        private readonly BindableCollection<IChannelViewModel> _channels = new BindableCollection<IChannelViewModel>();

        public ChannelsViewModel(IEnumerable<IChannelViewModel> channels)
        {
            _channels.AddRange(channels);
            SelectedChannel = Channels.FirstOrDefault();
        }

        public IEnumerable<IChannelViewModel> Channels => _channels;

        public IChannelViewModel SelectedChannel
        {
            get { return _selectedChannel; }
            set
            {
                if (Equals(value, _selectedChannel)) return;
                _selectedChannel = value;
                NotifyOfPropertyChange();
            }
        }
    }
}