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
                actual.ShouldAllBeEquivalentTo(expected);
            }
        }

        [Test]
        public void Preserve_after_wrap_around()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void Warn_before_wrap_around()
        {
            Assert.Fail("TODO");
        }
    }
}