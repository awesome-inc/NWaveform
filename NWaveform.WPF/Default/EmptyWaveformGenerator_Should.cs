using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;
using NWaveform.Model;

namespace NWaveform.Default
{
    [TestFixtureFor(typeof (EmptyWaveFormGenerator))]
    // ReSharper disable InconsistentNaming
    internal class EmptyWaveformGenerator_Should
    {
        [Test]
        public void Generate_Empty_Waveform()
        {
            var sut = new EmptyWaveFormGenerator();
            var actual = sut.Generate(null);
            actual.Should().NotBeNull();
            WaveformData.IsNullOrEmpty(actual).Should().BeTrue();
        }
    }
}