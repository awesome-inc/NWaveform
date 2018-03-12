using System;
using System.Diagnostics;
using System.Windows.Threading;
using Caliburn.Micro;
using NAudio.Wave;
using NWaveform.Exceptions;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.NAudio
{
    public class NAudioPlayer : PropertyChangedBase, IMediaPlayer, IDisposable
    {
        private readonly IWavePlayerFactory _playerFactory;
        private readonly IWaveProviderFactory _factory;
        public double MaxRate => 4;
        public double MinRate => 0.25;
        public double RateDelta => 0.25;

        private const double DefaultVolume = 0.5;
        private Uri _source;
        private IWavePlayer _player;
        private IWaveProviderEx _waveProvider;
        private readonly DispatcherTimer _positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private bool _isPlaying;
        private bool _isPaused;
        private bool _isStopped;
        private double _restoreVolume = DefaultVolume;

        public NAudioPlayer(IWaveProviderFactory waveFactory = null, IWavePlayerFactory playerFactory = null)
        {
            _factory = waveFactory ?? new WaveProviderFactory();
            _playerFactory = playerFactory ?? new WavePlayerFactory<WasapiOut>();
            _positionTimer.Interval = TimeSpan.FromSeconds(0.25);
            _positionTimer.Tick += PositionTimerTick;
            Error = AudioError.NoError;
        }

        public IPlayerError Error { get; }

        public void Play()
        {
            if (!CanPlay) return;

            _player.Play();
            _positionTimer.Start();

            IsPlaying = true;
            IsPaused = false;
            IsStopped = false;
        }

        public bool CanPlay => Source != null && !IsPlaying;

        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (_isPlaying == value) return;
                _isPlaying = value;
                NotifyOfPropertyChange();
                NotifyCanStates();
            }
        }

        public void Pause()
        {
            if (!CanPause) return;

            _player.Pause();
            _positionTimer.Stop();

            IsPlaying = false;
            IsPaused = true;
            IsStopped = false;
        }

        public bool CanPause => IsPlaying;

        public bool IsPaused
        {
            get => _isPaused;
            private set
            {
                _isPaused = value;
                NotifyOfPropertyChange();
                NotifyCanStates();
            }
        }

        private void NotifyCanStates()
        {
            NotifyOfPropertyChange(nameof(CanPlay));
            NotifyOfPropertyChange(nameof(CanPause));
            NotifyOfPropertyChange(nameof(CanStop));
        }

        public void Stop()
        {
            if (!CanStop) return;

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
            get => _isStopped;
            private set
            {
                _isStopped = value;
                NotifyOfPropertyChange();
                NotifyCanStates();
            }
        }


        public bool CanMute => Volume > 0;
        public void Mute() { _restoreVolume = Volume; Volume = 0.0; }
        public bool IsMuted => Volume < double.Epsilon;
        public bool CanUnMute => Volume < double.Epsilon;
        public void UnMute() { Volume = _restoreVolume; }

        public AudioSelection AudioSelection { get; set; }
        public bool CanLoop => false;
        public bool IsLooping => false;
        public void ToggleLoop() { }

        public Uri Source
        {
            get => _source;
            set
            {
                DisposeMedia();

                Open(value);

                _source = value;
                NotifyOfPropertyChange();

                NotifyOfPropertyChange(nameof(CanPlay));
                NotifyOfPropertyChange(nameof(CanStop));
                NotifyOfPropertyChange(nameof(CanPause));
                NotifyOfPropertyChange(nameof(Duration));
                NotifyOfPropertyChange(nameof(HasDuration));
                NotifyOfPropertyChange(nameof(Volume));
                NotifyOfPropertyChange(nameof(CanMute));
                NotifyOfPropertyChange(nameof(CanUnMute));
                NotifyOfPropertyChange(nameof(SupportsBalance));
            }
        }

        private void Open(Uri uri)
        {
            try
            {
                if (uri != null)
                {
                    _waveProvider = _factory.Create(uri);
                    _player = _playerFactory.Create();
                    _player.Init(_waveProvider);
                    _player.PlaybackStopped += OnStopped;
                }
                Error.Exception = null;
            }
            catch (Exception ex)
            {
                Error.Exception = new AudioException("Could not open audio", ex);
            }
        }

        public double Position
        {
            get => _waveProvider?.CurrentTime.TotalSeconds ?? 0.0;
            set
            {
                if (_source == null || _waveProvider == null) return;
                if (PositionCloseTo(_waveProvider.CurrentTime.TotalSeconds, value)) return;

                _waveProvider.CurrentTime = TimeSpan.FromSeconds(value);
                NotifyOfPropertyChange();
            }
        }

        public double Duration => _waveProvider?.TotalTime.TotalSeconds ?? 0.0;
        public bool HasDuration => Duration > 0.0;

        public void Faster() { Rate += RateDelta; }
        public void Slower() { Rate -= RateDelta; }

        public bool SupportsRate => false;
         #pragma warning disable
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        // TODO: implement speed using SoundTouch/Practice#, cf.:
        // - https://code.google.com/p/practicesharp/ 
        // - http://www.surina.net/soundtouch/ 
        public bool CanFaster => SupportsRate && Source != null && Rate < MaxRate;
        public bool CanSlower => SupportsRate && Source != null && Rate > MinRate;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        #pragma warning restore

        public double Rate
        {
            get => 1.0;
            set
            {
                // todo: not builtin to NAudio, use SoundTouch like Practice#, cf.: https://soundtouchdotnet.codeplex.com/
            }
        }

        public double Volume
        {
            get => _waveProvider?.Volume ?? DefaultVolume;
            set
            {
                if (_waveProvider == null) return;
                _waveProvider.Volume = (float)value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanMute));
                NotifyOfPropertyChange(nameof(CanUnMute));
                NotifyOfPropertyChange(nameof(IsMuted));
            }
        }

        public bool SupportsBalance => _waveProvider != null && _waveProvider.SupportsPanning;

        public double Balance
        {
            get => _waveProvider?.Pan ?? 0.0;
            set
            {
                if (_waveProvider == null) return;
                _waveProvider.Pan = (float)value;
                NotifyOfPropertyChange();
            }
        }

        public void Dispose()
        {
            DisposeMedia();
        }

        private void DisposeMedia()
        {
            Stop();

            if (_player != null) _player.PlaybackStopped -= OnStopped;
            SafeDispose(_player);
            _player = null;

            SafeDispose(_waveProvider);
            _waveProvider = null;
        }

        private static void SafeDispose(object obj)
        {
            (obj as IDisposable)?.Dispose();
        }

        private void OnStopped(object sender, StoppedEventArgs e)
        {
            HandleError(e.Exception);
            Stop();
        }

        private void HandleError(Exception exception)
        {
            if (exception != null)
            {
                var ex = new AudioException("Could not open audio", exception);
                Trace.TraceWarning(ex.ToString());
                Error.Exception = ex;
            }
            else
                Error.Exception = null;
        }

        [DebuggerStepThrough]
        private void PositionTimerTick(object sender, EventArgs e)
        {
            if (!IsPlaying) return;

            // HACK: workaround for NAudios PlaybackStopped event coming way too late (>1sec.)
            // also handles cases where Position > Duration (for wrap around buffers)
            if (Duration - Position <= TimeEpsilon)
            {
                Stop();
                return;
            }
            NotifyOfPropertyChange(nameof(Position));
        }

        private const double TimeEpsilon = 0.25;
        private bool PositionCloseTo(double a, double b)
        {
            return CloseTo(a, b, TimeEpsilon * Rate);
        }

        private static bool CloseTo(double a, double b, double epsilon)
        {
            return Math.Abs(a - b) <= epsilon;
        }
    }
}
