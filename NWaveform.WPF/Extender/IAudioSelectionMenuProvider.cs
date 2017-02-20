using NWaveform.ViewModels;

namespace NWaveform.Extender
{
    public interface IAudioSelectionMenuProvider
    {
        IMenuViewModel Menu { get; }
    }
}
