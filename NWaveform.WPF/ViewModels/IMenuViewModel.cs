using System.Collections.Generic;

namespace NWaveform.ViewModels
{
    public interface IMenuViewModel
    {
        IEnumerable<IMenuItemViewModel> Items { get; }
    }
}