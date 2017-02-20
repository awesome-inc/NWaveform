using System;
using FluentAssertions;
using NUnit.Framework;

namespace NWaveform.Model
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    internal class WaveformDataTests
    {
        [Test]
        public void TestIsNullOrEmpty()
        {
            WaveformData.IsNullOrEmpty(null).Should().BeTrue("waveform is null");

            var input = new WaveformData();
            WaveformData.IsNullOrEmpty(input).Should().BeTrue("waveform duration is zero");

            input.Duration = TimeSpan.FromSeconds(1);
            WaveformData.IsNullOrEmpty(input).Should().BeTrue("waveform has no channels");

            input.Channels = new[] {new Channel()};
            WaveformData.IsNullOrEmpty(input).Should().BeFalse();
        }
    }
}