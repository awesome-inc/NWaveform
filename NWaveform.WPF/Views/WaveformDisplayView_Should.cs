using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;
using NWaveform.ViewModels;

namespace NWaveform.Views
{

    [TestFixtureFor(typeof(WaveformDisplayView))]
    // ReSharper disable InconsistentNaming
    internal class WaveformDisplayView_Should
    {
        [Test]
        public void Provide_a_Waveform_Image()
        {
            typeof(WaveformDisplayView).Should().BeAssignableTo<IHaveWaveformImage>();
        }
    }
}
