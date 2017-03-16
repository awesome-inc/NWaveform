using System;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using FluentAssertions;
using NEdifis;
using NEdifis.Attributes;
using NUnit.Framework;
using NWaveform.Events;
using NWaveform.Model;

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

        [Test]
        public void Support_live_tracking()
        {
            var ctx = new ContextFor<WaveformDisplayViewModel>();
            var sut = ctx.BuildSut();

            sut.LiveTrackingEnabled.Should().BeTrue("by default live tracking should be enabled");
            sut.LastWritePosition.Should().Be(0);

            var peaks = new[]{ new PeakInfo(-1f,1f) };
            var uri = new Uri("channel://1/");
            var e = new PeaksReceivedEvent(uri, 0, 1, peaks);
            sut.Duration = e.End;
            sut.HandlePeaks(e);
            sut.LastWritePosition.Should().Be(e.End, "last write position should be updated for received peaks");


            sut.HandleShift(sut.LastWritePosition);
            sut.LastWritePosition.Should().Be(0);
        }

    }
}