using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
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
        , IHandleWithTask<PeaksReceivedEvent>
    {
        private PointCollection _userChannel = new PointCollection();
        private PointCollection _separationLeftChannel = new PointCollection();
        private PointCollection _separationRightChannel = new PointCollection();

        private double _duration;
        private IAudioSelectionViewModel _selection = new AudioSelectionViewModel();

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
        private SolidColorBrush _userTextBrush;
        private IPositionProvider _positionProvider = new EmptyPositionProvider();
        private WriteableBitmap _waveformImage = BitmapFactory.New(1920, 1080);

        int[] _leftChannel = new int[1920];
        int[] _rightChannel = new int[1920];

        internal WriteableBitmap WaveformImage
        {
            get { return _waveformImage; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _waveformImage = value;
                _waveformImage.Clear(BackgroundBrush.Color);
                Array.Resize(ref _leftChannel, (int)_waveformImage.Width);
                Array.Resize(ref _rightChannel, (int)_waveformImage.Height);
                Array.Clear(_leftChannel, 0, _leftChannel.Length);
                Array.Clear(_rightChannel, 0, _rightChannel.Length);
            }
        }

        public WaveformViewModel(IEventAggregator events, WaveformSettings waveformSettings = null)
        {
            var settings = waveformSettings ?? new WaveformSettings();

            _maxMagnitude = settings.MaxMagnitude;
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

        protected override void OnActivate()
        {
            RenderWaveform();
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

        public int[] LeftChannel => _leftChannel;
        public int[] RightChannel => _rightChannel;

        public void SetWaveform(WaveformData waveform)
        {
            Array.Clear(_leftChannel, 0, _leftChannel.Length);
            Array.Clear(_rightChannel, 0, _rightChannel.Length);

            if (waveform == null) return;

            var channels = waveform.Channels;
            if (channels.Length < 1 || channels.Length > 2)
                throw new InvalidOperationException("Only Mono/Stereo supported.");

            var duration = waveform.Duration.TotalSeconds;

            var leftPoints = GetPoints(channels[0].Samples, duration, true);
            var rightPoints = waveform.Channels.Length == 2
                ? GetPoints(channels[1].Samples, duration, false)
                : leftPoints.Scaled(1, -1); // mono? --> Y-flipped duplicate of left channel

            var sx = _leftChannel.Length / duration;
            var sy = _waveformImage.Height / 2.0;
            foreach (var point in leftPoints)
            {
                var x = (int)(sx * point.X);
                var y = (int) (sy * (1 - point.Y));
                _leftChannel[x] = y;
            }
            sx = (_rightChannel.Length - 1) / duration;
            foreach (var point in rightPoints)
            {
                var x = (int)(sx * point.X);
                var y = (int)(sy * (1 - point.Y));
                _rightChannel[x] = y;
            }

            Duration = duration;
        }

        public Task Handle(PeaksReceivedEvent message)
        {
            if (!SameSource(message)) return Task.FromResult(0);
            return Execute.OnUIThreadAsync(() => HandlePeaks(message));
        }

        internal void HandlePeaks(PeaksReceivedEvent message)
        {
            var pointsReceivedEvent = message.ToPoints(Duration, WaveformImage.Width, WaveformImage.Height);
            Handle(pointsReceivedEvent);
        }

        private void Handle(PointsReceivedEvent message)
        {
            for (int i = 0; i < message.LeftPoints.Length; i++) LeftChannel[message.XOffset + i] = message.LeftPoints[i];
            for (int i = 0; i < message.RightPoints.Length; i++) RightChannel[message.XOffset + i] = message.RightPoints[i];
            RenderPolyline(message.XOffset, message.LeftPoints, LeftBrush.Color);
            RenderPolyline(message.XOffset, message.RightPoints, RightBrush.Color);
        }

        private bool SameSource(PeaksReceivedEvent message)
        {
            return PositionProvider?.Source != null && message.Source == PositionProvider?.Source.ToString();
        }

        private void RenderWaveform()
        {
            WaveformImage.Clear(BackgroundBrush.Color);
            RenderPolyline(0, LeftChannel, LeftBrush.Color);
            RenderPolyline(0, RightChannel, RightBrush.Color);
            //Render(SeparationLeftChannel, SeparationLeftBrush.Color, ShapeMode.Bars);
            //Render(SeparationRightChannel, SeparationRightBrush.Color, ShapeMode.Bars);
            //Render(UserChannel, UserBrush.Color, ShapeMode.Bars);
        }


        private void RenderPolyline(int x0, int[] points, Color color)
        {
            //_waveformImage.DrawPolyline(points, color);

            var w = (int)WaveformImage.Width;
            var h = (int)WaveformImage.Height;
            var h2 = h / 2;
            var c = WriteableBitmapExtensions.ConvertColor(color);
            using (var ctx = WaveformImage.GetBitmapContext())
            {
                for (var i = 0; i < points.Length; i++)
                    WriteableBitmapExtensions.DrawLine(ctx, w, h, x0 + i, h2, x0 + i, points[i], c);
            }
        }

        private void RenderBars(PointCollection points, Color color)
        {
            if (points == null || points.Count < 4) return;
            var sx = WaveformImage.Width / Duration;
            var h2 = (int)(0.5 * WaveformImage.Height);

            var pts = new int[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                pts[2*i] = (int) (p.X * sx);
                pts[2*i+1] = (int)(h2 * (1 + p.Y));
            }

            var c = WriteableBitmapExtensions.ConvertColor(color);
            for (var i = 0; i < pts.Length - 4; i += 8)
                WaveformImage.FillRectangle(pts[i], pts[i + 1], pts[i + 4], pts[i + 5], c, true);
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