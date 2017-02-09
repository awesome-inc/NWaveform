using System.ComponentModel;
using NWaveform.Model;

namespace NWaveform.Interfaces
{
    public interface IPositionProvider : INotifyPropertyChanged
    {
        /// <summary>Gets or sets the current position of the media in seconds.</summary>
        /// <returns>The current position of the media (in seconds).</returns>
        double Position { get; set; }

        /// <summary>Gets the duration of the media.</summary>
        /// <returns>The duration of the media (in seconds). O if unknown like for e.g. continuous network streams.</returns>
        double Duration { get; }

        /// <summary> Gets or sets the audio selection. </summary>
        AudioSelection AudioSelection { get; set; }
    }
}