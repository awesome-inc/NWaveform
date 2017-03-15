using System;
using Caliburn.Micro;
using FluentAssertions;
using NAudio.Wave;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;
using NWaveform.Events;

namespace NWaveform.NAudio
{
    [TestFixtureFor(typeof(BufferedStreamingChannel))]
    // ReSharper disable InconsistentNaming
    internal class BufferedStreamingChannel_Should
    {
        [Test]
        [TestCase(1, 8000, 1)]
        public void Add_samples(double seconds, int rate, int channels)
        {
            var events = Substitute.For<IEventAggregator>();
            SamplesReceivedEvent samples = null;
            events.When(x => x.PublishOnCurrentThread(Arg.Any<SamplesReceivedEvent>()))
                .Do(x => samples = x.Arg<SamplesReceivedEvent>());

            var source = new Uri("some://uri");
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(rate, channels);
            var bufferSize = TimeSpan.FromSeconds(seconds);

            using (var sut = new BufferedStreamingChannel(events, source, waveFormat, bufferSize))
            {
                sut.Source.Should().Be(source);
                sut.Stream.TotalTime.Should().Be(bufferSize);

                var expected = waveFormat.Generate(bufferSize);
                sut.AddSamples(expected).Should().Be(expected.Length);

                var stream = sut.BufferedStream;
                stream.WritePosition.Should().Be(0, "wrap around");

                var actual = new byte[expected.Length];
                var numbytes = stream.Read(actual, 0, actual.Length);
                numbytes.Should().Be(expected.Length);
                actual.Should().Equal(expected);

                stream.Position.Should().Be(0, "wrap around");
            }

            samples.Should().NotBeNull("samples should be published");
            samples.Source.Should().Be(source, "wave data should be published using original uri");
            samples.Start.Should().Be(TimeSpan.Zero);
            samples.WaveFormat.Should().Be(waveFormat, "wave data should be published using original format");

            // TODO: test wrap around & splitted publishing
        }

        [Test]
        public void Shift_on_wrap_around()
        {
            var events = Substitute.For<IEventAggregator>();
            AudioShiftedEvent shifted = null;
            events.When(x => x.PublishOnCurrentThread(Arg.Any<AudioShiftedEvent>()))
                .Do(x => shifted = x.Arg<AudioShiftedEvent>());

            using (
                var sut = new BufferedStreamingChannel(events, new Uri("channel://1/"), new WaveFormat(8000, 8, 1),
                    TimeSpan.FromSeconds(2)) { PreserveAfterWrapAround = TimeSpan.FromSeconds(1)})
            {
                sut.Stream.CurrentTime = sut.PreserveAfterWrapAround;
                var startTime = sut.StartTime;

                sut.BufferedStream.OnWrappedAround();

                sut.Stream.CurrentTime.Should().Be(TimeSpan.Zero);
                sut.StartTime.Should().Be(startTime + sut.PreserveAfterWrapAround);
                shifted.Shift.Should().Be(sut.PreserveAfterWrapAround);
            }
        }
    }
}