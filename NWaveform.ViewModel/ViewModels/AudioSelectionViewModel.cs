using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NWaveform.ViewModels
{
    public class AudioSelectionViewModel : IAudioSelectionViewModel
    {
        private double _start;
        private double _end;
        private double _top = -1.0;
        private double _height = 2.0;
        private IMenuViewModel _menu;

        public double Start
        {
            get { return _start; }
            set
            {
                if (Math.Abs(_start - value) < double.Epsilon) return;
                _start = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("Duration");
            }
        }

        public double End
        {
            get { return _end; }
            set
            {
                if (Math.Abs(_end - value) < double.Epsilon) return;
                _end = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("Duration");
            }
        }

        public double Duration { get { return End - Start; } }

        public double Top
        {
            get { return _top; }
            set { _top = value; OnPropertyChanged(); }
        }

        public double Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged(); }
        }

        public IMenuViewModel Menu
        {
            get { return _menu; }
            set { _menu = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}