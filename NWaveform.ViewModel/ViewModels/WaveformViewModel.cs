using System;
using System.Collections.Generic;
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
        , IHandle<SamplesReceivedEvent>
        , IHandle<PointsReceivedEvent>
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

        internal readonly WriteableBitmap WaveformImage = BitmapFactory.New(1920, 1080);

        public WaveformViewModel(IMediaPlayer positionProvider,
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
                        case nameof(Position):
                            NotifyOfPropertyChange(e.PropertyName);
                            break;
                        case nameof(Duration):
                            Duration = _positionProvider.Duration;
                            break;
                    }
                };

            WaveformImage.Clear(BackgroundBrush.Color);
        }

        protected override void OnViewLoaded(object view)
        {
            var myView = view as WaveformView;
            if (myView != null)
                myView.WaveformImage.ImageSource = WaveformImage;
        }


        public double Position
        {
            get { return _positionProvider.Position; }
            set
            {
                var pos = Position;
                if (Math.Abs(pos - value) < double.Epsilon) return;
                _positionProvider.Position = value;
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
                _positionProvider.AudioSelection = new AudioSelection(0, _selection.Start, _selection.End);
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
            set { _backgroundBrush = value; NotifyOfPropertyChange(); }
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

        public void Handle(SamplesReceivedEvent message)
        {
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
                WaveformImage.FillQuad(points[i], h2, points[i], points[i + 1], points[i + 2], points[i + 3], points[i + 2], h2, color);
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