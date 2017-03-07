using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    public class WaveformViewModel : WaveformDisplayViewModel, IWaveformViewModel
    {
        private PointCollection _userChannel = new PointCollection();
        private PointCollection _separationLeftChannel = new PointCollection();
        private PointCollection _separationRightChannel = new PointCollection();

        private IAudioSelectionViewModel _selection = new AudioSelectionViewModel();

        private readonly double _maxMagnitude;
        private double _ticksEach;

        private SolidColorBrush _positionBrush;
        private SolidColorBrush _selectionBrush;

        private readonly BindableCollection<ILabelVievModel> _labels = new BindableCollection<ILabelVievModel>();
        private ILabelVievModel _selectedLabel;
        private IMenuViewModel _selectionMenu;
        private SolidColorBrush _userBrush;
        private SolidColorBrush _separationLeftBrush;
        private SolidColorBrush _separationRightBrush;
        private SolidColorBrush _userTextBrush;
        private IPositionProvider _positionProvider = new EmptyPositionProvider();

        private DateTimeOffset? _startTime;

        public WaveformViewModel(IEventAggregator events, WaveformSettings waveformSettings = null) : base(events)
        {
            var settings = waveformSettings ?? new WaveformSettings();

            _maxMagnitude = settings.MaxMagnitude;
            _ticksEach = settings.TicksEach;
            BackgroundBrush = new SolidColorBrush(settings.BackgroundColor);
            LeftBrush = new SolidColorBrush(settings.LeftColor);
            RightBrush = new SolidColorBrush(settings.RightColor);
            _positionBrush = new SolidColorBrush(settings.PositionColor);
            _selectionBrush = new SolidColorBrush(settings.SelectionColor);
            _userBrush = new SolidColorBrush(settings.UserColor);
            _separationLeftBrush = new SolidColorBrush(settings.SeparationLeftColor);
            _separationRightBrush = new SolidColorBrush(settings.SeparationRightColor);
            _userTextBrush = new SolidColorBrush(settings.UserTextColor);
        }

        public IPositionProvider PositionProvider
        {
            get { return _positionProvider; }
            set
            {
                if (Equals(value, _positionProvider)) return;
                _positionProvider.PropertyChanged -= PositionProviderNotifyOfPropertyChange;
                _positionProvider = value ?? new EmptyPositionProvider();
                _positionProvider.PropertyChanged += PositionProviderNotifyOfPropertyChange;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Position));
            }
        }

        internal override void HandleShift(double shift)
        {
            base.HandleShift(shift);

            Position -= shift;

            if (Selection != null)
            {
                Selection.Start -= shift;
                Selection.End -= shift;
            }
        }

        private void PositionProviderNotifyOfPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            // Position if a direct reference to _positionProvider whereby Duration caches the value in this instance
            switch (e.PropertyName)
            {
                case nameof(IPositionProvider.Position):
                    NotifyOfPropertyChange(nameof(Position));
                    break;
                case nameof(IPositionProvider.Duration):
                    Duration = PositionProvider.Duration;
                    break;
                case nameof(IPositionProvider.Source):
                    Source = PositionProvider.Source;
                    break;
            }
        }

        public double Position
        {
            get { return PositionProvider.Position; }
            set
            {
                var pos = Position;
                if (Math.Abs(pos - value) < double.Epsilon) return;
                PositionProvider.Position = value;
                NotifyOfPropertyChange();
            }
        }

        public bool HasDuration => Duration > 0.0;

        public IAudioSelectionViewModel Selection
        {
            get { return _selection; }
            set
            {
                _selection = value;
                // TODO: channel from top/height?!
                PositionProvider.AudioSelection = new AudioSelection(0, _selection.Start, _selection.End);
                NotifyOfPropertyChange();
            }
        }

        public IMenuViewModel SelectionMenu
        {
            get { return _selectionMenu; }
            set { _selectionMenu = value; NotifyOfPropertyChange(); }
        }

        public double TicksEach
        {
            get { return _ticksEach; }
            set { _ticksEach = value; NotifyOfPropertyChange(); }
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
            set { _selectedLabel = value; NotifyOfPropertyChange(); }
        }

        public SolidColorBrush UserBrush
        {
            get { return _userBrush; }
            set { _userBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public SolidColorBrush SeparationLeftBrush
        {
            get { return _separationLeftBrush; }
            set { _separationLeftBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public SolidColorBrush SeparationRightBrush
        {
            get { return _separationRightBrush; }
            set { _separationRightBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public SolidColorBrush UserTextBrush
        {
            get { return _userTextBrush; }
            set { _userTextBrush = value; NotifyOfPropertyChange(); }
        }

        public SolidColorBrush PositionBrush
        {
            get { return _positionBrush; }
            set { _positionBrush = value; NotifyOfPropertyChange(); }
        }

        public SolidColorBrush SelectionBrush
        {
            get { return _selectionBrush; }
            set { _selectionBrush = value; NotifyOfPropertyChange(); }
        }

        public PointCollection UserChannel
        {
            get { return _userChannel; }
            set { _userChannel = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public PointCollection SeparationLeftChannel
        {
            get { return _separationLeftChannel; }
            set { _separationLeftChannel = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public PointCollection SeparationRightChannel
        {
            get { return _separationRightChannel; }
            set { _separationRightChannel = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public void SetWaveform(WaveformData waveform)
        {
            WaveformImage.Clear(BackgroundBrush.Color);
            LeftChannel.Set(ZeroMagnitude);
            RightChannel.Set(ZeroMagnitude);

            if (waveform == null) return;

            var channels = waveform.Channels;
            if (channels.Length < 1 || channels.Length > 2)
                throw new InvalidOperationException("Only Mono/Stereo supported.");

            var duration = waveform.Duration.TotalSeconds;

            var leftPoints = GetPoints(channels[0].Samples, duration, true);
            var rightPoints = waveform.Channels.Length == 2
                ? GetPoints(channels[1].Samples, duration, false)
                : leftPoints.Scaled(1, -1); // mono? --> Y-flipped duplicate of left channel

            ResampleChannel(ZeroMagnitude, leftPoints, LeftChannel);
            ResampleChannel(ZeroMagnitude, rightPoints, RightChannel);
            Duration = duration;

            RenderWaveform();
        }

        private static void ResampleChannel(double sy, IList<Point> points, IList<int> channel)
        {
            if (points.Count < 3) return;
            var toT = (double)(points.Count-1) / (channel.Count - 1);
            for (var x = 0; x < channel.Count; x++)
            {
                var t = (int) (x * toT);
                t = Math.Max(0, Math.Min(t, points.Count - 1));

                var point = points[t];
                var y = (int)(sy * (1 + point.Y));
                channel[x] = y;
            }
        }

        private IList<Point> GetPoints(IList<float> samples, double duration, bool flipY)
        {
            var points = samples.ToPoints(); // get points in [0,Duration]...

            // flip and scale magnitude down to maxMagnitude
            points = points.Scaled(duration, flipY ? -_maxMagnitude : _maxMagnitude);

            return points;
        }
    }
}