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
            Source = source;
            Selection = selection ?? throw new ArgumentNullException(nameof(selection));
            OutputFilename = outputFilename;
        }
    }
}
