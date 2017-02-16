using System;
using System.Threading.Tasks;
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
        , IHandleWithTask<RefreshWaveformEvent>

    {
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
            Waveforms = waveforms;
            Waveform = waveform;
            Waveform.PositionProvider = player;

            var menu = audioSelectionMenuProvider?.Menu;
            if (menu != null) Waveform.SelectionMenu = menu;

            events.Subscribe(this);
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
                RefreshWaveform();
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

        public Task Handle(RefreshWaveformEvent message)
        {
            if (message.Source != Source) return Task.FromResult(0);
            return Execute.OnUIThreadAsync(RefreshWaveform);
        }

        private void RefreshWaveform()
        {
            Waveform.SetWaveform(GetWaveform());
        }
    }
}