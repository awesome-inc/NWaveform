using System;
using FluentAssertions;
using NAudio.Wave;
using NEdifis.Attributes;
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
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(rate, channels);
            var bufferSize = TimeSpan.FromSeconds(seconds);
            var source = new Uri("some://uri");
            using (var sut = new BufferedStreamingChannel(source, waveFormat, bufferSize))
            {
                sut.Source.Should().Be(source);
                sut.Stream.TotalTime.Should().Be(bufferSize);

                var time = TimeSpan.Zero;
                var expected = waveFormat.Generate(bufferSize);
                sut.AddSamples(time, expected);

                var stream = sut.BufferedStream;
                stream.WritePosition.Should().Be(0, "wrap around");

                var actual = new byte[expected.Length];
                var numbytes = stream.Read(actual, 0, actual.Length);
                numbytes.Should().Be(expected.Length);
                actual.Should().Equal(expected);

                stream.Position.Should().Be(0, "wrap around");
            }
        }

        [Test]
        public void Preserve_after_wrap_around()
        {
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8000, 1);
            var bufferSize = TimeSpan.FromSeconds(1);
            using (var sut = new BufferedStreamingChannel(new Uri("some://uri"), waveFormat, bufferSize))
            {
                sut.PreserveAfterWrapAround.Should().Be(TimeSpan.Zero, "default preserve should be zero (no preservation)");

                sut.Invoking(x => x.PreserveAfterWrapAround = bufferSize)
                    .ShouldThrow<ArgumentOutOfRangeException>("preserve must be less than buffer size");

                sut.PreserveAfterWrapAround = TimeSpan.FromSeconds(bufferSize.TotalSeconds * 0.75);

                // 1. no wrap around
                var expectedBytes = waveFormat.Generate(bufferSize);
                sut.AddSamples(TimeSpan.Zero, expectedBytes);

                var stream = sut.BufferedStream;
                var actualBytes = new byte[expectedBytes.Length];
                var numbytes = stream.Read(actualBytes, 0, actualBytes.Length);
                numbytes.Should().Be(actualBytes.Length);
                actualBytes.Should().Equal(expectedBytes, "no wrap around");

                // 2. wrap around: check data/position preserved after wrap around
                // position half-way between end & preserve
                stream.ClearBuffer();
                var skippedTime = sut.BufferSize - sut.PreserveAfterWrapAround;
                var time = TimeSpan.FromSeconds(0.5 * (skippedTime.TotalSeconds + sut.BufferSize.TotalSeconds));
                sut.Stream.CurrentTime = time;

                // create data that exceeds the buffer
                expectedBytes = waveFormat.Generate(sut.PreserveAfterWrapAround);
                Array.Resize(ref actualBytes, expectedBytes.Length);

                sut.MonitorEvents();
                sut.AddSamples(TimeSpan.Zero, expectedBytes);
                stream.WritePosition.Should().Be(expectedBytes.Length);
                sut.ShouldNotRaise(nameof(sut.WrappedAround));

                sut.AddSamples(TimeSpan.Zero, expectedBytes);
                sut.ShouldRaise(nameof(sut.WrappedAround));
                //stream.WritePosition.Should().Be(expectedBytes.Length, "preserved");

                sut.Stream.CurrentTime.Should().Be(time - skippedTime, "position should be wrapped");

                stream.Position = stream.WritePosition - expectedBytes.Length;
                stream.Read(actualBytes, 0, actualBytes.Length);
                actualBytes.Should().Equal(expectedBytes);
                stream.Position.Should().Be(actualBytes.Length);
            }
        }
    }
}