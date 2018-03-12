using System;
using System.Globalization;
using Caliburn.Micro;
using NAudio.Wave;
using NWaveform.ViewModels;

namespace NWaveform.App
{
    internal class ChannelViewModel : Screen, IChannelViewModel
    {
        private readonly IMixerChannel _channel;

        public ChannelViewModel(IWaveformDisplayViewModel waveform, IMixerChannel channel)
        {
            Waveform = waveform ?? throw new ArgumentNullException(nameof(waveform));
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            Waveform.PropertyChanged += Waveform_PropertyChanged;
        }

        public string StreamStartTime => Waveform.CurrentStreamTime.AddSeconds(-Waveform.Duration).ToString(CultureInfo.CurrentUICulture);
        public string Source => Waveform.Source?.ToString();
        public TimeSpan Duration => TimeSpan.FromSeconds(Waveform.Duration);
        public IWaveformDisplayViewModel Waveform { get; }

        public ISampleProvider MixerInput => _channel;
        public bool IsPlaying
        {
            get => _channel.IsPlaying;
            set
            {
                if (value == _channel.IsPlaying) return;
                _channel.IsPlaying = value;
                NotifyOfPropertyChange();
            }
        }

        public double Volume
        {
            get => _channel.Volume;
            set
            {
                if (value.Equals(_channel.Volume)) return;
                _channel.Volume = value;
                NotifyOfPropertyChange();
            }
        }

        public double Balance
        {
            get => _channel.Balance;
            set
            {
                if (value.Equals(_channel.Balance)) return;
                _channel.Balance = value;
                NotifyOfPropertyChange();
            }
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
    }
}
