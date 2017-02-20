using System;

namespace NWaveform
{
    public class CropAudioResponse
    {
        public Uri CroppedAudioUri { get; }

        public CropAudioResponse(Uri croppedAudioUri)
        {
            if (croppedAudioUri == null) throw new ArgumentNullException(nameof(croppedAudioUri));
            CroppedAudioUri = croppedAudioUri;
        }
    }
}