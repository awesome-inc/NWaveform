using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NWaveform.Default;
using NWaveform.Extender;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WaveformPlayerViewModel : IWaveformPlayerViewModel
    {
        // TODO: cleanup design so we can inject waveform without a dependency to the player. 
        // The problem here is the 3-way binding for Player <--> "Position" <--> Waveform
        // ReSharper disable once UnusedMember.Global
        public WaveformPlayerViewModel(
            IMediaPlayer player,
            IWaveFormRepository waveforms,
            IAudioSelectionMenuProvider audioSelectionMenuProvider = null)
            : this(player, waveforms, new PolygonWaveformViewModel(player), audioSelectionMenuProvider)
        {
        }

        // to make it testable we add this internal constructor here!
        internal WaveformPlayerViewModel(
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

            var menu = audioSelectionMenuProvider?.Menu;
            if (menu != null) Waveform.SelectionMenu = menu;
        }

        public IMediaPlayer Player { get; private set; }
        public IWaveFormRepository Waveforms { get; private set; }
        public IWaveformViewModel Waveform { get; private set; }

        public Uri Source
        {
            get { return Player.Source; }
            set
            {
                if (Player.Source == value) return;
                Player.Source = value;
                OnPropertyChanged();
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

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}