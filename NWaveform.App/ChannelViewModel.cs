using System;
using System.Globalization;
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
                case nameof(IWaveformDisplayViewModel.CurrentStreamTime):
                    NotifyOfPropertyChange(nameof(StreamStartTime));
                    break;
            }
        }

        public string StreamStartTime => Waveform.CurrentStreamTime.AddSeconds(-Waveform.Duration).ToString(CultureInfo.CurrentUICulture);
        public string Source => Waveform.Source?.ToString();
        public TimeSpan Duration => TimeSpan.FromSeconds(Waveform.Duration);
        public IWaveformDisplayViewModel Waveform { get; }
    }
}