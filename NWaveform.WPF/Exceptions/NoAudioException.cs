using System;

namespace NWaveform.Exceptions
{
    public sealed class NoAudioException : AudioException
    {
        public NoAudioException(string message, Exception innerException) : base(message, innerException)
        {}

        public NoAudioException(string message) : base(message)
        {}

        public NoAudioException()
        {}
    }
}