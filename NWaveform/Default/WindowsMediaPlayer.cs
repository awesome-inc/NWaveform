using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Threading;
using NWaveform.Exceptions;
using NWaveform.Extensions;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.Default
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class WindowsMediaPlayer : IMediaPlayer
    {
        private const double RateEpsilon = 0.125;

        public double MaxRate => 4;
        public double MinRate => 0.25;
        public double RateDelta => 0.25;

        private readonly MediaPlayer _player = new MediaPlayer();
        private readonly DispatcherTimer _positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private bool _isPlaying;
        private bool _isPaused;
        private bool _isStopped;
        private double _restoreVolume = 0.5;
        private double _rate = 1.0;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public WindowsMediaPlayer()
        {
            _player.MediaOpened += PlayerMediaOpened;

            _player.MediaEnded += (s, e) => Stop();
            _player.MediaFailed += (s, e) => HandleMediaFailedException(e.ErrorException);

            _positionTimer.Interval = TimeSpan.FromMilliseconds(250);
            _positionTimer.Tick += PositionTimerTick;

            Error = AudioError.NoError;
        }

        private void PlayerMediaOpened(object sender, EventArgs e)
        {
            Error.Exception = null;

            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged(nameof(Source));
            OnPropertyChanged(nameof(Duration));
            OnPropertyChanged(nameof(HasDuration));
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public double Duration => HasDuration ? _player.NaturalDuration.TimeSpan.TotalSeconds : 0.0;

        public bool HasDuration => _player.NaturalDuration.HasTimeSpan;

        public double Position
        {
            get { return _player.Position.TotalSeconds; }
            set
            {
                _player.Position = TimeSpan.FromSeconds(value);
                OnPropertyChanged();
            }
        }

        public double Volume
        {
            get { return _player.Volume; }
            set
            {
                _player.Volume = value;
                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(CanMute));
                OnPropertyChanged(nameof(CanUnMute));
                OnPropertyChanged(nameof(IsMuted));
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        public bool SupportsBalance => true;
        public double Balance
        {
            get { return _player.Balance; }
            set
            {
                _player.Balance = value;
                OnPropertyChanged();
            }
        }

        public AudioSelection AudioSelection { get; set; }
        public bool CanLoop => false;
        public bool IsLooping => false;
        public void ToggleLoop() { }

        public bool SupportsRate => true;
        public void Faster() { Rate += RateDelta; }
        public bool CanFaster => Source != null && Rate < MaxRate;
        public void Slower() { Rate -= RateDelta; }
        public bool CanSlower => Source != null && Rate > MinRate;

        public double Rate
        {
            get { return _rate; }
            set
            {
                var newValue = Math.Max(MinRate, Math.Min(MaxRate, value));
                if (CloseTo(_rate, newValue, RateEpsilon)) return;
                _player.SpeedRatio = (float)newValue;
                _rate = newValue;
                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(CanFaster));
                OnPropertyChanged(nameof(CanSlower));
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        private static bool CloseTo(double a, double b, double epsilon)
        {
            return Math.Abs(a - b) <= epsilon;
        }

        public Uri Source
        {
            get { return _player.Source; }
            set
            {
                VerifyUri(value);
                _player.Open(value);
                Stop();
                OnPropertyChanged();
            }
        }

        private void VerifyUri(Uri uri)
        {
            try
            {
                uri?.VerifyUriExists();
                Error.Exception = null;
            }
            catch (Exception ex) { Error.Exception = new AudioException("Could not open audio", ex); }
        }

        public IPlayerError Error { get; }

        public void Play()
        {
            _player.Play();
            _positionTimer.Start();

            IsPlaying = true;
            IsPaused = false;
            IsStopped = false;
        }

        public bool CanPlay => Source != null && !IsPlaying;

        public bool IsPlaying
        {
            get { return _isPlaying; }
            private set
            {
                _isPlaying = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("CanPlay");
            }
        }

        public void Pause()
        {
            _player.Pause();
            _positionTimer.Stop();

            IsPlaying = false;
            IsPaused = true;
            IsStopped = false;
        }

        public bool CanPause => _player.CanPause;

        public bool IsPaused
        {
            get { return _isPaused; }
            private set
            {
                _isPaused = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("CanPause");
            }
        }

        public void Stop()
        {
            _player.Stop();
            _positionTimer.Stop();

            IsPlaying = false;
            IsPaused = false;
            IsStopped = true;

            Position = 0.0;
        }

        public bool CanStop => IsPlaying || IsPaused;

        public bool IsStopped
        {
            get { return _isStopped; }
            private set
            {
                _isStopped = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("CanStop");
            }
        }

        public bool CanMute => Volume > 0;
        public void Mute() { _restoreVolume = Volume; Volume = 0.0; }
        public bool IsMuted => CanUnMute;
        public bool CanUnMute => Volume < double.Epsilon;
        public void UnMute() { Volume = _restoreVolume; }

        [DebuggerStepThrough]
        private void PositionTimerTick(object sender, EventArgs e)
        {
            if (!IsPlaying) return;
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged("Position");
        }

        private void HandleMediaFailedException(Exception exception)
        {
            Error.Exception = new AudioException("Could not open audio", exception);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
