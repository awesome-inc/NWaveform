using System;
using Caliburn.Micro;
using NWaveform.Events;
using NWaveform.Extender;
using NWaveform.Interfaces;

namespace NWaveform.ViewModels
{
    public class WaveformPlayerViewModel : Screen
        , IWaveformPlayerViewModel, IDisposable
    {
        public DateTimeOffset? StartTime
        {
            get { return _startTime; }
            set
            {
                if (value.Equals(_startTime)) return;
                _startTime = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(HasCurrentTime));
                NotifyOfPropertyChange(nameof(CurrentTime));
            }
        }

        private readonly IAbsoluteTimeFormatter _formatTime;
        private readonly IGetTimeStamp _getTime;
        private DateTimeOffset? _startTime;
        private readonly IEventAggregator _events;

        public WaveformPlayerViewModel(IEventAggregator events,
            IMediaPlayer player,
            IWaveformViewModel waveform,
            IAudioSelectionMenuProvider audioSelectionMenuProvider,
            IAbsoluteTimeFormatter formatTime,
            IGetTimeStamp getTime)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (waveform == null) throw new ArgumentNullException(nameof(waveform));
            if (formatTime == null) throw new ArgumentNullException(nameof(formatTime));
            if (getTime == null) throw new ArgumentNullException(nameof(getTime));

            Player = player;
            Player.PropertyChanged += Player_PropertyChanged;
            Waveform = waveform;
            _formatTime = formatTime;
            _getTime = getTime;
            _events = events;

            Waveform.PositionProvider = player;
            Waveform.ConductWith(this);

            var menu = audioSelectionMenuProvider?.Menu;
            if (menu != null) Waveform.SelectionMenu = menu;

            _events.Subscribe(this);
        }

        private void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (StartTime.HasValue && e.PropertyName == nameof(Player.Position))
                NotifyOfPropertyChange(nameof(CurrentTime));
        }

        public IMediaPlayer Player { get; set; }
        public IWaveformViewModel Waveform { get; set; }

        public Uri Source
        {
            get { return Player.Source; }
            set
            {
                if (Player.Source == value) return;
                Player.Source = value;
                Waveform.Source = value;
                NotifyOfPropertyChange();
                StartTime = value != null ? _getTime.For(value) : null;
            }
        }

        public bool HasCurrentTime => StartTime.HasValue;
        public string CurrentTime => _formatTime.Format(StartTime + TimeSpan.FromSeconds(Player.Position));

        public void Handle(AudioShiftedEvent message)
        {
            if (!SameSource(message.Source)) return;
            if (StartTime.HasValue)
                StartTime = StartTime.Value + message.Shift;
        }

        private bool SameSource(Uri source)
        {
            return Source != null && Source == source;
        }

        protected override void OnDeactivate(bool close)
        {
            Player?.Pause();
            base.OnDeactivate(close);
        }

        public void Dispose()
        {
            _events.Unsubscribe(this);
            Player = null;
            Waveform = null;
       }
    }
}