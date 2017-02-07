using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    public class PolygonWaveformViewModel : IWaveformViewModel
    {
        private readonly IMediaPlayer _positionProvider;
        private PointCollection _leftChannel;
        private PointCollection _rightChannel;
        private double _duration;
        private IAudioSelectionViewModel _selection = new AudioSelectionViewModel();

        private readonly double _maxError;
        private readonly double _maxMagnitude;
        private double _ticksEach;

        private SolidColorBrush _backgroundBrush;
        private SolidColorBrush _leftBrush;
        private SolidColorBrush _rightBrush;
        private SolidColorBrush _positionBrush;
        private SolidColorBrush _selectionBrush;


        private readonly BindableCollection<ILabelVievModel> _labels = new BindableCollection<ILabelVievModel>();
        private ILabelVievModel _selectedLabel;
        private IMenuViewModel _selectionMenu;
        private SolidColorBrush _userBrush;
        private SolidColorBrush _separationLeftBrush;
        private SolidColorBrush _separationRightBrush;
        private PointCollection _userChannel;
        private SolidColorBrush _userTextBrush;
        private PointCollection _separationLeftChannel;
        private PointCollection _separationRightChannel;

        public PolygonWaveformViewModel(IMediaPlayer positionProvider,
            WaveformSettings waveformSettings = null)
        {
            if (positionProvider == null) throw new ArgumentNullException(nameof(positionProvider));
            var settings = waveformSettings ?? new WaveformSettings();

            _maxMagnitude = settings.MaxMagnitude;
            _maxError = settings.MaxError;
            _ticksEach = settings.TicksEach;
            _backgroundBrush = new SolidColorBrush(settings.BackgroundColor);
            _leftBrush = new SolidColorBrush(settings.LeftColor);
            _rightBrush = new SolidColorBrush(settings.RightColor);
            _positionBrush = new SolidColorBrush(settings.PositionColor);
            _selectionBrush = new SolidColorBrush(settings.SelectionColor);
            _userBrush = new SolidColorBrush(settings.UserColor);
            _separationLeftBrush = new SolidColorBrush(settings.SeparationLeftColor);
            _separationRightBrush = new SolidColorBrush(settings.SeparationRightColor);
            _userTextBrush = new SolidColorBrush(settings.UserTextColor);

            _positionProvider = positionProvider;

            // hook up into player's position changed event to fake-notify our view that our own virtual position has changed!
            _positionProvider.PropertyChanged += (s, e) =>
                {
                    // Position if a direct reference to _positionProvider whereby Duration caches the value in this instance
                    switch (e.PropertyName)
                    {
                        case "Position":
                            // ReSharper disable once ExplicitCallerInfoArgument
                            OnPropertyChanged(e.PropertyName);
                            break;
                        case "Duration":
                            Duration = _positionProvider.Duration;
                            break;
                        // ReSharper disable once RedundantEmptyDefaultSwitchBranch
                        default:
                            // ReSharper disable once EmptyStatement
                            ; break;
                    }
                };
        }

        public double Position
        {
            get { return _positionProvider.Position; }
            set
            {
                var pos = Position;
                if (Math.Abs(pos - value) < Double.Epsilon) return;
                _positionProvider.Position = value;
                OnPropertyChanged();
            }
        }

        public double Duration
        {
            get { return _duration; }
            set
            {
                if (Math.Abs(_duration - value) < double.Epsilon) return;
                _duration = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("HasDuration");
            }
        }

        public bool HasDuration { get { return Duration > 0.0; } }

        public IAudioSelectionViewModel Selection
        {
            get { return _selection; }
            set
            {
                _selection = value;
                // TODO: channel from top/height?!
                _positionProvider.AudioSelection = new AudioSelection(0, _selection.Start, _selection.End);
                OnPropertyChanged();
            }
        }

        public IMenuViewModel SelectionMenu
        {
            get { return _selectionMenu; }
            set { _selectionMenu = value; OnPropertyChanged(); }
        }

        public double TicksEach
        {
            get { return _ticksEach; }
            set { _ticksEach = value; OnPropertyChanged(); }
        }

        public IList<ILabelVievModel> Labels
        {
            get { return _labels; }
            set
            {
                _labels.Clear();
                if (value != null)
                    _labels.AddRange(value);
            }
        }

        public ILabelVievModel SelectedLabel
        {
            get { return _selectedLabel; }
            set { _selectedLabel = value; OnPropertyChanged(); }
        }

        public SolidColorBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { _backgroundBrush = value; OnPropertyChanged(); }
        }

        public SolidColorBrush LeftBrush
        {
            get { return _leftBrush; }
            set { _leftBrush = value; OnPropertyChanged(); }
        }

        public SolidColorBrush RightBrush
        {
            get { return _rightBrush; }
            set { _rightBrush = value; OnPropertyChanged(); }
        }

        public SolidColorBrush UserBrush
        {
            get { return _userBrush; }
            set { _userBrush = value; OnPropertyChanged(); }
        }

        public SolidColorBrush SeparationLeftBrush
        {
            get { return _separationLeftBrush; }
            set { _separationLeftBrush = value; OnPropertyChanged(); }
        }

        public SolidColorBrush SeparationRightBrush
        {
            get { return _separationRightBrush; }
            set { _separationRightBrush = value; OnPropertyChanged(); }
        }
        
        public SolidColorBrush UserTextBrush
        {
            get { return _userTextBrush; }
            set { _userTextBrush = value; OnPropertyChanged(); }
        }

        public SolidColorBrush PositionBrush
        {
            get { return _positionBrush; }
            set { _positionBrush = value; OnPropertyChanged(); }
        }

        public SolidColorBrush SelectionBrush
        {
            get { return _selectionBrush; }
            set { _selectionBrush = value; OnPropertyChanged(); }
        }

        public PointCollection LeftChannel
        {
            get { return _leftChannel; }
            private set { _leftChannel = value; OnPropertyChanged(); }
        }

        public PointCollection RightChannel
        {
            get { return _rightChannel; }
            private set { _rightChannel = value; OnPropertyChanged(); }
        }

        public PointCollection UserChannel
        {
            get { return _userChannel; }
            set { _userChannel = value; OnPropertyChanged(); }
        }

        public PointCollection SeparationLeftChannel
        {
            get { return _separationLeftChannel; }
            set { _separationLeftChannel = value; OnPropertyChanged(); }
        }

        public PointCollection SeparationRightChannel
        {
            get { return _separationRightChannel; }
            set { _separationRightChannel = value; OnPropertyChanged(); }
        }

        public void SetWaveform(WaveformData waveform)
        {
            if (waveform == null)
            {
                LeftChannel = new PointCollection();
                RightChannel = new PointCollection();
                return;
            }

            var channels = waveform.Channels;
            if (channels.Length < 1 || channels.Length > 2)
                throw new InvalidOperationException("Only Mono/Stereo supported.");

            var duration = waveform.Duration.TotalSeconds;
            var leftPoints = GetPoints(channels[0].Samples, duration, true);
            var rightPoints = waveform.Channels.Length == 2
                ? GetPoints(channels[1].Samples, duration, false)
                : leftPoints.Scaled(1, -1); // mono? --> Y-flipped duplicate of left channel

            LeftChannel = new PointCollection(leftPoints);
            RightChannel = new PointCollection(rightPoints);
            Duration = duration;
        }

        private IList<Point> GetPoints(IList<float> samples, double duration, bool flipY)
        {
            var points = samples.ToPoints(); // get points in [0,Duration]...

            if (_maxError > double.Epsilon) // simplify mainly in X=[0,Duration]-space!
            {
                var normalizedError = _maxError / duration;
                points = points.Simplified(normalizedError);
            }

            // flip and scale magnitude down to maxMagnitude
            points = points.Scaled(duration, flipY ? -_maxMagnitude : _maxMagnitude);

            return points;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}