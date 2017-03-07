using System;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using FluentAssertions;
using NEdifis;
using NEdifis.Attributes;
using NUnit.Framework;
using NWaveform.Events;

namespace NWaveform.ViewModels
{
    [TestFixtureFor(typeof(WaveformDisplayViewModel))]
    // ReSharper disable InconsistentNaming
    internal class WaveformDisplayViewModel_Should
    {
        [Test]
        public void Handle_shift_events()
        {
            var ctx = new ContextFor<WaveformDisplayViewModel>();
            var sut = ctx.BuildSut();
            sut.Should().BeAssignableTo<IHandle<AudioShiftedEvent>>();

            var source = new Uri("http://some/audio/");
            var shift = TimeSpan.FromSeconds(3);
            var e = new AudioShiftedEvent(source, shift);
            sut.Source.Should().BeNull("skip shift if not same source");
            sut.Handle(e);

            sut.Source = source;
            sut.WaveformImage = BitmapFactory.New(20, 20);
            sut.Duration.Should().Be(0, "skip shifts if duration 0");
            sut.Handle(e);

            sut.Duration = shift.TotalSeconds * 2;
            sut.HandleShift(e.Shift.TotalSeconds);
        }
    }
}