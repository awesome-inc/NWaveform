using System;
using NWaveform.ViewModels;

namespace NWaveform
{
    public class CropAudioRequest
    {
        public Uri Source { get; }
        public IAudioSelectionViewModel Selection { get; }
        public string OutputFilename { get; }

        public CropAudioRequest(Uri source, IAudioSelectionViewModel selection, string outputFilename = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selection == null) throw new ArgumentNullException(nameof(selection));
            Source = source;
            Selection = selection;
            OutputFilename = outputFilename;
        }
    }
}