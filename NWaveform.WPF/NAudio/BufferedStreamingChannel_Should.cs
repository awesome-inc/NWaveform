using System;
using Caliburn.Micro;
using FluentAssertions;
using NAudio.Wave;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;

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

            events.Received().PublishOnCurrentThread(Arg.Any<SamplesReceivedEvent>());

            // TODO: test wrap around & publishing
        }
    }
}