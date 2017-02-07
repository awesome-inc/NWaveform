using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NWaveform.Interfaces;

namespace NWaveform.Model
{
    public class AudioError : IPlayerError
    {
        public static readonly AudioError NoError = new AudioError();
        private Exception _exception;

        public AudioError(Exception exception = null)
        {
            Exception = exception;
        }

        public Exception Exception
        {
            get { return _exception; }
            set
            {
                if (_exception == value) return;
                _exception = value;
                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(HasException));
                OnPropertyChanged(nameof(Message));
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        public bool HasException => _exception != null;
        public string Message => _exception?.Message ?? string.Empty;

        public event PropertyChangedEventHandler PropertyChanged = delegate {};

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}