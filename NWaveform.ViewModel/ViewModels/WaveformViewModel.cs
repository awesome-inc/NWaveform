using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using NWaveform.Events;
using NWaveform.Interfaces;
using NWaveform.Model;
using NWaveform.Views;

namespace NWaveform.ViewModels
{
    public class WaveformViewModel : Screen, IWaveformViewModel
        , IHandle<PeaksReceivedEvent>
        , IHandle<PointsReceivedEvent>
    {
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
        private IPositionProvider _positionProvider = new EmptyPositionProvider();

        internal WriteableBitmap WaveformImage { get; set; } = BitmapFactory.New(1920, 1080);

        public WaveformViewModel(IEventAggregator events, WaveformSettings waveformSettings = null)
        {
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

            WaveformImage.Clear(BackgroundBrush.Color);
            events.Subscribe(this);
        }

        public IPositionProvider PositionProvider
        {
            get { return _positionProvider; }
            set
            {
                if (Equals(value, _positionProvider)) return;
                _positionProvider.PropertyChanged -= PositionProviderOnPropertyChanged;
                _positionProvider = value ?? new EmptyPositionProvider();
                _positionProvider.PropertyChanged += PositionProviderOnPropertyChanged;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Position));
            }
        }

        private void PositionProviderOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Position if a direct reference to _positionProvider whereby Duration caches the value in this instance
            switch (e.PropertyName)
            {
                case nameof(Position):
                    NotifyOfPropertyChange(nameof(Position));
                    break;
                case nameof(Duration):
                    Duration = PositionProvider.Duration;
                    break;
            }
        }

        protected override void OnViewLoaded(object view)
        {
            var myView = view as WaveformView;
            if (myView != null)
                myView.WaveformImage.ImageSource = WaveformImage;
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

        public double Duration
        {
            get { return _duration; }
            set
            {
                if (Math.Abs(_duration - value) < double.Epsilon) return;
                _duration = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(HasDuration));
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

        public SolidColorBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { _backgroundBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public SolidColorBrush LeftBrush
        {
            get { return _leftBrush; }
            set { _leftBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public SolidColorBrush RightBrush
        {
            get { return _rightBrush; }
            set { _rightBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
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

        public PointCollection LeftChannel
        {
            get { return _leftChannel; }
            private set { _leftChannel = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public PointCollection RightChannel
        {
            get { return _rightChannel; }
            private set { _rightChannel = value; NotifyOfPropertyChange(); RenderWaveform(); }
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

            Duration = duration;

            LeftChannel = new PointCollection(leftPoints);
            RightChannel = new PointCollection(rightPoints);
        }

        public void Handle(PeaksReceivedEvent message)
        {
            if (!SameSource(message)) return;

            Handle(message.ToPoints(Duration, WaveformImage.Width, WaveformImage.Height));
            //var leftPoints = GetPoints(channels[0].Samples, Duration, true);
            //var rightPoints = waveform.Channels.Length == 2
            //    ? GetPoints(channels[1].Samples, duration, false)
            //    : leftPoints.Scaled(1, -1); // mono? --> Y-flipped duplicate of left channel

            //RenderPolyline(points, LeftBrush.Color);
        }

        public void Handle(PointsReceivedEvent message)
        {
            RenderPolyline(message.LeftPoints, LeftBrush.Color);
            RenderPolyline(message.RightPoints, RightBrush.Color);
        }

        private bool SameSource(PeaksReceivedEvent message)
        {
            return PositionProvider?.Source != null && message.Source == PositionProvider?.Source.ToString();
        }

        private void RenderWaveform()
        {
            WaveformImage.Clear(BackgroundBrush.Color);

            Render(LeftChannel, LeftBrush.Color);
            Render(RightChannel, RightBrush.Color);
            Render(SeparationLeftChannel, SeparationLeftBrush.Color, ShapeMode.Bars);
            Render(SeparationRightChannel, SeparationRightBrush.Color, ShapeMode.Bars);
            Render(UserChannel, UserBrush.Color, ShapeMode.Bars);
        }

        enum ShapeMode { Polyline, Bars };

        private void Render(PointCollection points, Color color, ShapeMode shapeMode = ShapeMode.Polyline)
        {
            if (points == null || points.Count < 4) return;
            var sx = WaveformImage.Width / Duration;
            var h2 = (int)(0.5 * WaveformImage.Height);

            var pts = new int[points.Count * 2];
            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                pts[2*i] = (int) (p.X * sx);
                pts[2*i+1] = (int)(h2 * (1 + p.Y));
            }
            switch (shapeMode)
            {
                case ShapeMode.Bars: RenderBars(pts, color); break;
                default: RenderPolyline(pts, color); break;
            }
        }

        private void RenderPolyline(int[] points, Color color)
        {
            //_waveformImage.DrawPolyline(points, color);

            var h2 = (int)(0.5 * WaveformImage.Height);
            for (var i = 0; i < points.Length - 2; i += 2)
                WaveformImage.FillQuad(
                    points[i], h2, 
                    points[i], points[i + 1], 
                    points[i + 2], points[i + 3], 
                    points[i + 2], h2, 
                    color);
        }

        private void RenderBars(int[] points, Color color)
        {
            var c = WriteableBitmapExtensions.ConvertColor(color);
            for (var i = 0; i < points.Length - 4; i += 8)
                WaveformImage.FillRectangle(points[i], points[i + 1], points[i + 4], points[i + 5], c, true);
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
    }
}