using NWaveform.Interfaces;

namespace NWaveform.ViewModels
{
    public interface IHavePlayer
    {
        IMediaPlayer Player { get; }
    }
}