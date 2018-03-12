using System;
using Caliburn.Micro;
using NWaveform.Interfaces;

namespace NWaveform.Model
{
    public class AudioError : PropertyChangedBase, IPlayerError
    {
        public static readonly AudioError NoError = new AudioError();
        private Exception _exception;

        public AudioError(Exception exception = null)
        {
            Exception = exception;
        }

        public Exception Exception
        {
            get => _exception;
            set
            {
                if (_exception == value) return;
                _exception = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(HasException));
                NotifyOfPropertyChange(nameof(Message));
            }
        }

        public bool HasException => _exception != null;
        public string Message => _exception?.Message ?? string.Empty;
    }
}
