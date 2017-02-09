using System;
using Caliburn.Micro;
using NWaveform.Default;
using NWaveform.Extender;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    public class WaveformPlayerViewModel : Screen, IWaveformPlayerViewModel
    {
        public WaveformPlayerViewModel(
            IMediaPlayer player,
            IWaveFormRepository waveforms,
            IWaveformViewModel waveform,
            IAudioSelectionMenuProvider audioSelectionMenuProvider)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (waveforms == null) throw new ArgumentNullException(nameof(waveforms));
            if (waveform == null) throw new ArgumentNullException(nameof(waveform));

            Player = player;
            Waveforms = waveforms;
            Waveform = waveform;
            Waveform.PositionProvider = player;

            var menu = audioSelectionMenuProvider?.Menu;
            if (menu != null) Waveform.SelectionMenu = menu;
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
    }
}