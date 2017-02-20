using System;
using Caliburn.Micro;
using FluentAssertions;
using NAudio.Wave;
using NEdifis;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;
using NWaveform.Events;
using NWaveform.NAudio;

namespace NWaveform
{
    [TestFixtureFor(typeof(SamplesHandlerPeakPublisher))]
    // ReSharper disable InconsistentNaming
    internal class SamplesHandlerPeakPublisher_Should
    {
        [Test]
        public void Handle_samples_and_publish_peas()
        {
            var ctx = new ContextFor<SamplesHandlerPeakPublisher>();
            var sut = ctx.BuildSut();

            sut.Should().BeAssignableTo<IHandle<SamplesReceivedEvent>>();
            var events = ctx.For<IEventAggregator>();
            events.Received().Subscribe(sut);

            var waveFormat = new WaveFormat(8000, 1);
            var start = TimeSpan.Zero;
            var duration = TimeSpan.FromSeconds(1);
            var data = waveFormat.Generate(duration);
            var samples = new SamplesReceivedEvent(new Uri("some://uri/"), start, waveFormat, data);

            PeaksReceivedEvent actualPeaks = null;
            events.When(x => x.PublishOnCurrentThread(Arg.Any<PeaksReceivedEvent>())).Do(x => actualPeaks = x.Arg<PeaksReceivedEvent>());

            var peakProvider = ctx.For<IPeakProvider>();
            WaveFormat actualWaveformat = null;
            byte[] actualData = null;
            peakProvider.When(x => x.Sample(Arg.Any<WaveFormat>(), Arg.Any<byte[]>())).Do(x =>
            {
                actualWaveformat = x.Arg<WaveFormat>();
                actualData = x.Arg<byte[]>();
            });

            sut.Handle(samples);

            actualPeaks.Source.Should().Be(samples.Source);
            actualPeaks.Start.Should().Be(samples.Start.TotalSeconds);
            actualPeaks.End.Should().Be((samples.Start + duration).TotalSeconds);

            actualWaveformat.Should().BeSameAs(waveFormat);
            actualData.Should().BeSameAs(samples.Data);
        }
    }
}