using System;
using NWaveform.ViewModels;

namespace NWaveform.App
{
    public class CropAudioRequest
    {
        public Uri Source { get; }
        public IAudioSelectionViewModel Selection { get; }

        public CropAudioRequest(Uri source, IAudioSelectionViewModel selection)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selection == null) throw new ArgumentNullException(nameof(selection));
            Source = source;
            Selection = selection;
        }
    }
}