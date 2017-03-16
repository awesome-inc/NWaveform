using System;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using NWaveform.Events;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    public class WaveformDisplayViewModel : Screen, IWaveformDisplayViewModel
    {
        private readonly IGetWaveform _getWaveform;
        private double _duration;
        private WriteableBitmap _waveformImage;
        private int[] _leftChannel;
        private int[] _rightChannel;
        private int _width;

        private int ZeroMagnitude { get; set; }

        private SolidColorBrush _backgroundBrush = new SolidColorBrush(WaveformSettings.DefaultBackgroundColor);
        private SolidColorBrush _leftBrush = new SolidColorBrush(WaveformSettings.DefaultLeftColor);
        private SolidColorBrush _rightBrush = new SolidColorBrush(WaveformSettings.DefaultRightColor);
        private Uri _source;
        private bool _liveTrackingEnabled = true;
        private double _lastWritePosition;
        private SolidColorBrush _lastWriteBrush = new SolidColorBrush(WaveformSettings.DefaultTransparentBlack);

        public WaveformDisplayViewModel(IEventAggregator events, IGetWaveform getWaveform)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (getWaveform == null) throw new ArgumentNullException(nameof(getWaveform));
            _getWaveform = getWaveform;
            WaveformImage = BitmapFactory.New(1920, 1080);
            events.Subscribe(this);
        }

        internal WriteableBitmap WaveformImage
        {
            get { return _waveformImage; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _waveformImage = value;
                ZeroMagnitude = (int) (_waveformImage.Height / 2.0);
                _width = (int) _waveformImage.Width;
                if (_leftChannel == null) _leftChannel = new int[_width];
                else Array.Resize(ref _leftChannel, _width);
                if (_rightChannel == null) _rightChannel = new int[_width];
                else Array.Resize(ref _rightChannel, _width);
                _leftChannel.Set(ZeroMagnitude);
                _rightChannel.Set(ZeroMagnitude);
                _waveformImage.Clear(BackgroundBrush.Color);
            }
        }

        public Uri Source
        {
            get { return _source; }
            set
            {
                if (Equals(value, _source)) return;
                _source = value;
                SetWaveform(_source);
                NotifyOfPropertyChange();
            }
        }

        private void SetWaveform(Uri source)
        {
            _waveformImage.Clear(BackgroundBrush.Color);
            Duration = 0.0;
            if (source == null) return;

            var waveform = _getWaveform.For(source);
            if (WaveformData.IsNullOrEmpty(waveform)) return;

            Duration = waveform.Duration.TotalSeconds;
            var e = new PeaksReceivedEvent(source, 0, Duration, waveform.Peaks);
            HandlePeaks(e);
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

        public SolidColorBrush LeftBrush
        {
            get { return _leftBrush; }
            set
            {
                _leftBrush = value;
                NotifyOfPropertyChange();
                RenderWaveform();
            }
        }

        public SolidColorBrush RightBrush
        {
            get { return _rightBrush; }
            set
            {
                _rightBrush = value;
                NotifyOfPropertyChange();
                RenderWaveform();
            }
        }

        public int[] LeftChannel => _leftChannel;
        public int[] RightChannel => _rightChannel;

        public SolidColorBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set
            {
                _backgroundBrush = value;
                NotifyOfPropertyChange();
                RenderWaveform();
            }
        }

        public bool HasDuration => Duration > 0.0;

        public bool LiveTrackingEnabled
        {
            get { return _liveTrackingEnabled; }
            set
            {
                if (value == _liveTrackingEnabled) return;
                _liveTrackingEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public double LastWritePosition
        {
            get { return _lastWritePosition; }
            private set
            {
                if (value.Equals(_lastWritePosition)) return;
                _lastWritePosition = value;
                NotifyOfPropertyChange();
            }
        }

        public SolidColorBrush LastWriteBrush
        {
            get { return _lastWriteBrush; }
            set
            {
                if (Equals(value, _lastWriteBrush)) return;
                _lastWriteBrush = value;
                NotifyOfPropertyChange();
            }
        }

        protected override void OnViewLoaded(object view)
        {
            var myView = view as IHaveWaveformImage;
            if (myView != null)
                myView.WaveformImageBrush.ImageSource = WaveformImage;
        }

        protected override void OnActivate()
        {
            RenderWaveform();
        }

        public void Handle(PeaksReceivedEvent message)
        {
            if (!SameSource(message.Source)) return;
            Execute.OnUIThread(() => HandlePeaks(message));
        }

        public void Handle(AudioShiftedEvent message)
        {
            if (!SameSource(message.Source)) return;
            Execute.OnUIThread(() => HandleShift(message.Shift.TotalSeconds));
        }

        protected internal void HandlePeaks(PeaksReceivedEvent message)
        {
            var pointsReceivedEvent = message.ToPoints(Duration, WaveformImage.Width, WaveformImage.Height);
            Handle(pointsReceivedEvent);
#if DEBUG
            Trace.WriteLine(
                $"Received #{message.Peaks.Length} peaks ({message.Start}:{message.End}) for '{message.Source}' ");
#endif
            LastWritePosition = message.End;
        }

        protected internal virtual void HandleShift(double shift)
        {
            if (Duration < double.Epsilon) return;
            var dx = (int)(WaveformImage.Width * shift / Duration);
            if (dx == 0) return; // no shift

            var x0 = (dx > 0 ? dx : 0);
            var x1 = (dx < 0 ? -dx : 0);
            var count = WaveformImage.PixelWidth - Math.Abs(dx);

            Array.Copy(_leftChannel, x0, _leftChannel, x1, count);
            Array.Copy(_rightChannel, x0, _rightChannel, x1, count);
            // zero tail of points
            var x2 = x1 + count;
            _leftChannel.Set(ZeroMagnitude, x2);
            _rightChannel.Set(ZeroMagnitude, x2);

            _waveformImage.Clear(BackgroundBrush.Color);
            RenderWaveform();
#if DEBUG
            Trace.WriteLine($"Shifted audio {shift} seconds.");
#endif
            LastWritePosition -= shift;
        }

        private void Handle(PointsReceivedEvent message)
        {
            _leftChannel.FlushedCopy(message.XOffset, message.LeftPoints, ZeroMagnitude);
            _rightChannel.FlushedCopy(message.XOffset, message.RightPoints, ZeroMagnitude);
            RenderWaveform(message.XOffset);
        }

        private bool SameSource(Uri source)
        {
            return Source != null && source == Source;
        }

        protected void RenderWaveform(int x0 = 0, int len = 0)
        {
            var w = (int) WaveformImage.Width;
            var h = (int) WaveformImage.Height;
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
    }
}