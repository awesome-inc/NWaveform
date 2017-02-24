using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;
using NWaveform.ViewModels;

namespace NWaveform.Views
{
    [TestFixtureFor(typeof(WaveformView))]
    // ReSharper disable InconsistentNaming
    internal class WaveformView_Should
    {
        [Test]
        public void Provide_a_Waveform_Image()
        {
            typeof(WaveformView).Should().BeAssignableTo<IHaveWaveformImage>();
        }
    }
}