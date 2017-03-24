using System.Windows.Input;
using FontAwesome.Sharp;

namespace NWaveform.ViewModels
{
    public interface IMenuItemViewModel : IMenuViewModel
    {
        IconChar Icon { get; set; }
        string Header { get; set; }
        ICommand Command { get; }
        string Description { get; set; }
    }
}