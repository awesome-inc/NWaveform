using System;
using Caliburn.Micro;
using NWaveform.ViewModels;

namespace NWaveform.App
{
    internal class ChannelViewModel : Screen, IChannelViewModel
    {
        public ChannelViewModel(IWaveformDisplayViewModel waveform)
        {
            if (waveform == null) throw new ArgumentNullException(nameof(waveform));
            Waveform = waveform;
            Waveform.PropertyChanged += Waveform_PropertyChanged;
        }

        private void Waveform_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IWaveformDisplayViewModel.Source):
                    NotifyOfPropertyChange(nameof(Source));
                    break;
                case nameof(IWaveformDisplayViewModel.Duration):
                    NotifyOfPropertyChange(nameof(Duration));
                    break;
            }
        }

        public string Source => Waveform.Source?.ToString();
        public TimeSpan Duration => TimeSpan.FromSeconds(Waveform.Duration);
        public IWaveformDisplayViewModel Waveform { get; }
    }
}