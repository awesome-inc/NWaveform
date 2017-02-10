using System;
using Caliburn.Micro;
using NWaveform.Events;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class SamplesHandlerPeakPublisher : IHandle<SamplesReceivedEvent>
    {
        private readonly IEventAggregator _events;
        private readonly IPeakProvider _peakProvider;

        public SamplesHandlerPeakPublisher(IEventAggregator events, IPeakProvider peakProvider)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (peakProvider == null) throw new ArgumentNullException(nameof(peakProvider));
            _events = events;
            _peakProvider = peakProvider;
            _events.Subscribe(this);
        }

        public void Handle(SamplesReceivedEvent samples)
        {
            var start = samples.Start.TotalSeconds;
            var end = start + ((double)samples.Data.Length / samples.WaveFormat.AverageBytesPerSecond);
            var peaks = _peakProvider.Sample(samples.WaveFormat, samples.Data);
            _events.PublishOnCurrentThread(new PeaksReceivedEvent(samples.Source, start,end, peaks));
        }
    }
}