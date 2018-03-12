using System;
using Caliburn.Micro;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class MixerChannel : IMixerChannel, IHandle<SamplesReceivedEvent>
    {
        public static readonly WaveFormat DefaultFormat = WaveFormat.CreateIeeeFloatWaveFormat(8000, 2);

        private readonly IEventAggregator _events;
        private readonly BufferedWaveProvider _buffer;
        private readonly VolumeSampleProvider _volume;
        private readonly PanningSampleProvider _balance;

        public MixerChannel(IEventAggregator events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));

            _buffer = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(WaveFormat.SampleRate, 1));
            _volume = new VolumeSampleProvider(_buffer.ToSampleProvider());
            _balance = new PanningSampleProvider(_volume) { Pan = 0f };

            _events.Subscribe(this);
        }

        public WaveFormat WaveFormat { get; } = DefaultFormat;
        public int Read(float[] buffer, int offset, int count)
        {
            return _balance.Read(buffer, offset, count);
        }

        public bool IsPlaying { get; set; }

        public double Volume
        {
            get => _volume?.Volume ?? 0.0;
            set { if (_volume != null) _volume.Volume = (float)value; }
        }

        public double Balance
        {
            get => _balance?.Pan ?? 0.0;
            set { if (_balance != null) _balance.Pan = (float)value; }
        }

        public Uri Source { get; set; }

        public void Handle(SamplesReceivedEvent message)
        {
            if (!IsPlaying || !SameSource(message.Source)) return;
            var data = message.WaveFormat.Resample(message.Data, _buffer.WaveFormat);
            _buffer.AddSamples(data, 0, data.Length);
        }

        private bool SameSource(Uri source)
        {
            return Source != null && source == Source;
        }
    }
}
