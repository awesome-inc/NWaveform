using System;

namespace NWaveform.App
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