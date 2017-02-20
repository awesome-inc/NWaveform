using System.Collections.Generic;
using NWaveform.ViewModels;

namespace NWaveform.Extender
{
    public interface IAudioSelectionMenuExtender
    {
        IEnumerable<IMenuItemViewModel> MenuItems { get; }
    }
}