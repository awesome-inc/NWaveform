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

        private WriteableBitmap _waveformImage;
        private int[] _leftChannel;
        private int[] _rightChannel;
        private int _width;
        private int _zeroMagnitude;
        private DateTimeOffset? _startTime;

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

            WaveformImage = BitmapFactory.New(1920, 1080);

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

        internal WriteableBitmap WaveformImage
        {
            get { return _waveformImage; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _waveformImage = value;
                _zeroMagnitude = (int)(_waveformImage.Height / 2.0);
                _width = (int)_waveformImage.Width;
                if (_leftChannel == null) _leftChannel = new int[_width]; else Array.Resize(ref _leftChannel, _width);
                if (_rightChannel == null) _rightChannel = new int[_width]; else Array.Resize(ref _rightChannel, _width);
                ArrayExtensions.Set(_leftChannel, _zeroMagnitude);
                ArrayExtensions.Set(_rightChannel, _zeroMagnitude);
                _waveformImage.Clear(BackgroundBrush.Color);
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

        public DateTimeOffset? StartTime
        {
            get { return _startTime; }
            set
            {
                if (value.Equals(_startTime)) return;
                _startTime = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CurrentTime));
            }
        }

        public DateTimeOffset? CurrentTime => StartTime + TimeSpan.FromSeconds(Position);

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
            _waveformImage.Clear(BackgroundBrush.Color);
            _leftChannel.Set(_zeroMagnitude);
            _rightChannel.Set(_zeroMagnitude);

            if (waveform == null) return;

            var channels = waveform.Channels;
            if (channels.Length < 1 || channels.Length > 2)
                throw new InvalidOperationException("Only Mono/Stereo supported.");

            var duration = waveform.Duration.TotalSeconds;

            var leftPoints = GetPoints(channels[0].Samples, duration, true);
            var rightPoints = waveform.Channels.Length == 2
                ? GetPoints(channels[1].Samples, duration, false)
                : leftPoints.Scaled(1, -1); // mono? --> Y-flipped duplicate of left channel

            ResampleChannel(_zeroMagnitude, leftPoints, _leftChannel);
            ResampleChannel(_zeroMagnitude, rightPoints, _rightChannel);
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
            _leftChannel.FlushedCopy(message.XOffset, message.LeftPoints, _zeroMagnitude);
            _rightChannel.FlushedCopy(message.XOffset, message.RightPoints, _zeroMagnitude);
            RenderWaveform(message.XOffset);
        }

        private bool SameSource(PeaksReceivedEvent message)
        {
            return PositionProvider?.Source != null && message.Source == PositionProvider?.Source;
        }

        private void RenderWaveform(int x0 = 0, int len = 0)
        {
            var w = (int)WaveformImage.Width;
            var h = (int)WaveformImage.Height;
            var h2 = h / 2;

            // clear background from x0 to x1
            var x1 = len > 0 ? x0 + len : w;
            var backColor = WriteableBitmapExtensions.ConvertColor(BackgroundBrush.Color);
            WaveformImage.FillRectangle(x0, 0, x1, h, backColor);

            using (var ctx = WaveformImage.GetBitmapContext())
            {
                var leftColor = WriteableBitmapExtensions.ConvertColor(LeftBrush.Color);
                var rightColor = WriteableBitmapExtensions.ConvertColor(RightBrush.Color);
                for (var x = x0; x < _leftChannel.Length; x++)
                {
                    WriteableBitmapExtensions.DrawLine(ctx, w, h, x, h2, x, _leftChannel[x], leftColor);
                    WriteableBitmapExtensions.DrawLine(ctx, w, h, x, h2, x, _rightChannel[x], rightColor);
                }
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