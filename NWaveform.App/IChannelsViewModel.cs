using System.Collections.Generic;
using Caliburn.Micro;

namespace NWaveform.App
{
    public interface IChannelsViewModel : IScreen
    {
        IEnumerable<IChannelViewModel> Channels { get; }
        IChannelViewModel SelectedChannel { get; set; }
    }
}