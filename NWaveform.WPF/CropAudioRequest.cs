using System;
using NWaveform.ViewModels;

namespace NWaveform
{
    public class CropAudioRequest
    {
        public IAudioSelectionViewModel Selection { get; }
        public string OutputFilename { get; }

        public CropAudioRequest(IAudioSelectionViewModel selection, string outputFilename = null)
        {
            Selection = selection ?? throw new ArgumentNullException(nameof(selection));
            OutputFilename = outputFilename;
        }
    }
}
