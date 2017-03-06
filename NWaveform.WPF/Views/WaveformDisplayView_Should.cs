using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluentAssertions;
using NEdifis;
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

        [Test, Issue("https://github.com/awesome-inc/NWaveform/issues/1")]
        public void Clear_waveform_image_when_updating_source()
        {
            var ctx = new ContextFor<WaveformDisplayViewModel>();
            var sut = ctx.BuildSut();

            sut.WaveformImage.Clear(Colors.Red);
            sut.Source = new Uri("http://some/audio");

            sut.WaveformImage.ShouldHaveColor(sut.BackgroundBrush.Color);
        }
    }
}
