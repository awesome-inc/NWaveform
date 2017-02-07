using System;

namespace NWaveform.Exceptions
{
    public class AudioException : Exception
    {
        public AudioException(string message, Exception innerException) : base(message, innerException)
        {}

        public AudioException(string message) : base(message)
        {}

        public AudioException()
        {}
    }
}