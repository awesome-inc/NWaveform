using System;
using Caliburn.Micro;

namespace NWaveform.ViewModels
{
    public class AudioSelectionViewModel: PropertyChangedBase, IAudioSelectionViewModel
    {
        private double _start;
        private double _end;
        private double _top = -1.0;
        private double _height = 2.0;
        private IMenuViewModel _menu;

        public void Copy(IAudioSelectionViewModel value)
        {
            if (value == null)
            {
                Start = End = 0;
                return;
            }

            if (value.Source != Source) throw new InvalidOperationException("Cannot change source");
            Start = value.Start;
            End = value.End;
            Top = value.Top;
            Height = value.Height;
        }

        public Uri Source { get; set; }

        public double Start
        {
            get => _start;
            set
            {
                if (Math.Abs(_start - value) < double.Epsilon) return;
                _start = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Duration));
            }
        }

        public double End
        {
            get => _end;
            set
            {
                if (Math.Abs(_end - value) < double.Epsilon) return;
                _end = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Duration));
            }
        }

        public double Duration => End - Start;

        public double Top
        {
            get => _top;
            set { _top = value; NotifyOfPropertyChange(); }
        }

        public double Height
        {
            get => _height;
            set { _height = value; NotifyOfPropertyChange(); }
        }

        public IMenuViewModel Menu
        {
            get => _menu;
            set { _menu = value; NotifyOfPropertyChange(); }
        }
    }
}
