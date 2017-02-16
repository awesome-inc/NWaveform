using System;
using System.Linq;
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
            using (var sut = new BufferedStreamingChannel("name", waveFormat, bufferSize))
            {
                sut.Name.Should().Be("name");
                sut.Stream.TotalTime.Should().Be(bufferSize);

                var time = TimeSpan.Zero;
                var expected = waveFormat.Generate(bufferSize);
                sut.AddSamples(time, expected);

                var stream = sut.BufferedStream;
                stream.BufferedDuration.Should().Be(stream.BufferDuration);

                var actual = new byte[expected.Length];
                var numbytes = stream.Read(actual, 0, actual.Length);
                numbytes.Should().Be(expected.Length);
                actual.Should().Equal(expected);
            }
        }

        [Test]
        [TestCase(0.5)]
        public void Preserve_after_wrap_around(double preserveFactor)
        {
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8000, 1);
            var bufferSize = TimeSpan.FromSeconds(2);
            using (var sut = new BufferedStreamingChannel("name", waveFormat, bufferSize))
            {
                sut.PreserveAfterWrapAround.Should().Be(TimeSpan.Zero, "default preserve should be zero (no preservation)");

                sut.Invoking(x => x.PreserveAfterWrapAround = bufferSize)
                    .ShouldThrow<ArgumentOutOfRangeException>("preserve must be less than buffer size");

                sut.PreserveAfterWrapAround = TimeSpan.FromSeconds(bufferSize.TotalSeconds * preserveFactor);

                var data = waveFormat.Generate(bufferSize);

                sut.AddSamples(TimeSpan.Zero, data);

                var preserved = (int)(data.Length * sut.PreserveAfterWrapAround.TotalSeconds / sut.BufferSize.TotalSeconds);
                var skipped = data.Length - preserved;
                var expected = data.Skip(skipped).Take(preserved).Concat(Enumerable.Repeat((byte) 0, skipped)).ToArray();

                var stream = sut.BufferedStream;
                var actual = new byte[expected.Length];
                var numbytes = stream.Read(actual, 0, actual.Length);
                numbytes.Should().Be(expected.Length);
                actual.Should().Equal(expected);
            }
        }

        [Test]
        public void Warn_before_wrap_around()
        {
            Assert.Fail("TODO");
        }
    }
}