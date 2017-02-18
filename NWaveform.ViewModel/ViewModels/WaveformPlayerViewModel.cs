using System;
using Caliburn.Micro;
using NWaveform.Default;
using NWaveform.Extender;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    public class WaveformPlayerViewModel : Screen
        , IWaveformPlayerViewModel
    {
        private DateTimeOffset? _startTime = DateTimeOffset.UtcNow;

        public WaveformPlayerViewModel(IEventAggregator events,
            IMediaPlayer player,
            IWaveFormRepository waveforms,
            IWaveformViewModel waveform,
            IAudioSelectionMenuProvider audioSelectionMenuProvider)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (waveforms == null) throw new ArgumentNullException(nameof(waveforms));
            if (waveform == null) throw new ArgumentNullException(nameof(waveform));

            Player = player;
            Player.PropertyChanged += Player_PropertyChanged;
            Waveforms = waveforms;
            Waveform = waveform;
            Waveform.PositionProvider = player;

            var menu = audioSelectionMenuProvider?.Menu;
            if (menu != null) Waveform.SelectionMenu = menu;

            events.Subscribe(this);
        }

        private void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_startTime.HasValue && e.PropertyName == nameof(Player.Position))
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
            }
        }

        public WaveformData GetWaveform()
        {
            var waveform = Waveforms.For(Player.Source);
            if (waveform == null) return EmptyWaveFormGenerator.CreateEmpty(Player.Duration);

            if (waveform.Duration == TimeSpan.Zero && Player.HasDuration)
                waveform.Duration = TimeSpan.FromSeconds(Player.Duration);
            return waveform;
        }

        public bool HasCurrentTime => _startTime.HasValue;
        public DateTimeOffset? CurrentTime => _startTime + TimeSpan.FromSeconds(Player.Position);
    }
}