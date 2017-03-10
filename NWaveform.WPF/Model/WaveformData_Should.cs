using System;
using FluentAssertions;
using NUnit.Framework;

namespace NWaveform.Model
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    internal class WaveformData_Should
    {
        [Test]
        public void Check_IsNullOrEmpty()
        {
            WaveformData.IsNullOrEmpty(null).Should().BeTrue("waveform is null");

            var input = new WaveformData(TimeSpan.Zero);
            WaveformData.IsNullOrEmpty(input).Should().BeTrue("waveform duration is zero");

            input = new WaveformData(TimeSpan.FromSeconds(1));
            WaveformData.IsNullOrEmpty(input).Should().BeTrue("waveform has no peaks");

            input = new WaveformData(TimeSpan.FromSeconds(1), new [] {new PeakInfo(-1,1)});
            WaveformData.IsNullOrEmpty(input).Should().BeFalse();
        }
    }
}