using System;
using System.IO;
using Caliburn.Micro;
using NWaveform.Default;
using NWaveform.Events;
using NWaveform.Extender;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    public class WaveformPlayerViewModel : Screen
        , IWaveformPlayerViewModel
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
        private DateTimeOffset? _startTime;

        public WaveformPlayerViewModel(IEventAggregator events,
            IMediaPlayer player,
            IWaveFormRepository waveforms,
            IWaveformViewModel waveform,
            IAudioSelectionMenuProvider audioSelectionMenuProvider,
            IAbsoluteTimeFormatter formatTime)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (waveforms == null) throw new ArgumentNullException(nameof(waveforms));
            if (waveform == null) throw new ArgumentNullException(nameof(waveform));
            if (formatTime == null) throw new ArgumentNullException(nameof(formatTime));

            Player = player;
            Player.PropertyChanged += Player_PropertyChanged;
            Waveforms = waveforms;
            Waveform = waveform;
            _formatTime = formatTime;
            Waveform.PositionProvider = player;

            var menu = audioSelectionMenuProvider?.Menu;
            if (menu != null) Waveform.SelectionMenu = menu;

            events.Subscribe(this);
        }

        private void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (StartTime.HasValue && e.PropertyName == nameof(Player.Position))
                NotifyOfPropertyChange(nameof(CurrentTime));
        }

        public IMediaPlayer Player { get; }
        public IWaveFormRepository Waveforms { get; }
        public IWaveformViewModel Waveform { get; }

        public Uri Source
        {
            get { return Player.Source; }
            set
            {
                if (Player.Source == value) return;
                Player.Source = value;
                NotifyOfPropertyChange();
                Waveform.SetWaveform(GetWaveform());
                if (value != null && value.IsFile) StartTime = new FileInfo(value.AbsolutePath).LastWriteTimeUtc;
            }
        }

        public WaveformData GetWaveform()
        {
            var waveform = Player.Source != null ? Waveforms.For(Player.Source) : null;
            if (waveform == null) return EmptyWaveFormGenerator.CreateEmpty(Player.Duration);

            if (waveform.Duration == TimeSpan.Zero && Player.HasDuration)
                waveform.Duration = TimeSpan.FromSeconds(Player.Duration);
            return waveform;
        }

        public bool HasCurrentTime => StartTime.HasValue;
        public string CurrentTime => _formatTime.Format(StartTime + TimeSpan.FromSeconds(Player.Position));

        public void Handle(StartTimeChanged message)
        {
            var sameSource = Source != null && Source == message.Source;
            if (!sameSource) return;

            StartTime = message.StartTime;
        }
    }
}