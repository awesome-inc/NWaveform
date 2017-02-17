using System;
using FluentAssertions;
using NAudio.Wave;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.NAudio
{
    [TestFixtureFor(typeof(BufferedWaveStream))]
    // ReSharper disable InconsistentNaming
    internal class BufferedWaveStream_Should
    {
        [Test]
        public void Clamp_positions()
        {
            var waveFormat = new WaveFormat(8000, 1);
            var duration = TimeSpan.FromSeconds(1);

            var sut = new BufferedWaveStream(waveFormat, duration);
            sut.BufferDuration.Should().Be(duration);
            sut.Length.Should().Be(sut.BufferLength);
            sut.BufferLength.Should().Be((int) (waveFormat.AverageBytesPerSecond * duration.TotalSeconds));

            sut.Position.Should().Be(0);
            sut.Position = sut.BufferLength+1;
            sut.Position.Should().Be(sut.BufferLength);

            sut.Position = -1;
            sut.Position.Should().Be(0);
        }
    }
}