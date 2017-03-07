using System;
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
            sut.Source = source;
            var shift = TimeSpan.FromSeconds(-3);
            var e = new AudioShiftedEvent(source, shift);
            sut.Handle(e);
        }
    }
}