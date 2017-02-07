using System;

namespace NWaveform.Exceptions
{
    public sealed class AudioNotFoundException : AudioException
    {
        public AudioNotFoundException(string message, Exception innerException) : base(message, innerException)
        {}

        public AudioNotFoundException(string message) : base(message)
        {}

        public AudioNotFoundException()
        {}
    }
}