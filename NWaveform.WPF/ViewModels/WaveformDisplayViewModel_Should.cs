using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using FluentAssertions;
using NEdifis;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;
using NWaveform.Events;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    [TestFixtureFor(typeof(WaveformDisplayViewModel))]
    // ReSharper disable InconsistentNaming
    internal class WaveformDisplayViewModel_Should
    {
        [Test]
        public void Update_waveform_when_changing_source()
        {
            var ctx = new ContextFor<WaveformDisplayViewModel>();
            var sut = ctx.BuildSut();

            var uri = new Uri("my://source/");
            var getWaveform = ctx.For<IGetWaveform>();
            var waveform = new WaveformData(TimeSpan.FromSeconds(2), new[]{new PeakInfo(-1f,1) });
            getWaveform.For(uri).Returns(waveform);

            sut.Source = uri;

            getWaveform.Received().For(uri);
            sut.CurrentStreamTime.Should().BeCloseTo(DateTime.UtcNow, 200);
            sut.Duration.Should().Be(waveform.Duration.TotalSeconds);
        }

        [Test]
        public void Handle_live_sample_updates()
        {
            var ctx = new ContextFor<WaveformDisplayViewModel>();
            var sut = ctx.BuildSut();

            sut.Should().BeAssignableTo<IHandle<PeaksReceivedEvent>>();
            ctx.For<IEventAggregator>().Received().Subscribe(sut);

            // |----------         |
            // |-------------------|
            // |          ---------|   
            sut.BackgroundBrush = new SolidColorBrush(Colors.Black);
            sut.LeftBrush = new SolidColorBrush(Colors.Red);
            sut.RightBrush = new SolidColorBrush(Colors.Blue);
            sut.WaveformImage = BitmapFactory.New(20, 20);
            var w2 = (int)(sut.WaveformImage.Width / 2);
            var h2 = (int)(sut.WaveformImage.Height / 2);

            var peaks = Enumerable.Repeat(1f, 2 * w2).Concat(Enumerable.Repeat(0f, 2 * w2))
                .Select(m => new PeakInfo(m - 1f, m)).ToArray();
            var e = new PeaksReceivedEvent(new Uri("source://test/"), 0, 2, peaks);
            sut.Duration = e.End;
            sut.HandlePeaks(e);

            // assert the image rendered
            sut.WaveformImage.RectShouldHaveColor(1, 1, w2, h2, sut.LeftBrush.Color);
            sut.WaveformImage.RectShouldHaveColor(w2 + 1, h2 + 1, 2 * w2, 2 * h2, sut.RightBrush.Color);
            sut.WaveformImage.RectShouldHaveColor(1, h2 + 1, w2 - 1, 2 * h2 - 1, sut.BackgroundBrush.Color);
            sut.WaveformImage.RectShouldHaveColor(w2 + 1, 1, 2 * w2, h2, sut.BackgroundBrush.Color);

            // assert the points are filled
            sut.LeftChannel.Take(10).Should().AllBeEquivalentTo(0);
            sut.RightChannel.Take(10).Should().AllBeEquivalentTo(h2);
            sut.LeftChannel.Skip(10).Take(10).Should().AllBeEquivalentTo(h2);
            sut.RightChannel.Skip(10).Take(10).Should().AllBeEquivalentTo(2 * h2);
        }

        [Test]
        public void Only_Handle_events_for_same_source()
        {
            var ctx = new ContextFor<WaveformDisplayViewModel>();
            var sut = ctx.BuildSut();

            sut.BackgroundBrush = new SolidColorBrush(Colors.Black);
            sut.WaveformImage = BitmapFactory.New(20, 20);
            var e = new PeaksReceivedEvent(new Uri("source://test/"), 0, 1, new PeakInfo[0]);
            sut.Source = new Uri("other://source/");
            sut.Handle(e);
            sut.WaveformImage.RectShouldHaveColor(0, 0, 20, 20, sut.BackgroundBrush.Color);
        }

        [Test]
        public void Refresh_waveform_when_receiving_earlier_samples_than_before()
        {
            var ctx = new ContextFor<WaveformDisplayViewModel>();
            var sut = ctx.BuildSut();

            sut.BackgroundBrush = new SolidColorBrush(Colors.Black);
            var color = Colors.Red;
            sut.LeftBrush = sut.RightBrush = new SolidColorBrush(color);
            sut.WaveformImage = BitmapFactory.New(30, 20);

            var uri = new Uri("source://test/");
            var w3 = (int)(sut.WaveformImage.Width / 3);
            var peaks = Enumerable.Repeat(1f, w3).Select(m => new PeakInfo(-m, m)).ToArray();

            sut.Duration = 3;
            // 1/3
            var e = new PeaksReceivedEvent(uri, 0, 1, peaks);
            sut.HandlePeaks(e);
            sut.WaveformImage.RectShouldHaveColor(0, 0, w3 - 1, 20, color);
            sut.WaveformImage.RectShouldHaveColor(w3, 0, 3 * w3, 20, sut.BackgroundBrush.Color);
            // 3/3
            e = new PeaksReceivedEvent(uri, 2, 3, peaks);
            sut.HandlePeaks(e);
            sut.WaveformImage.RectShouldHaveColor(0, 0, w3 - 1, 20, color);
            sut.WaveformImage.RectShouldHaveColor(w3, 0, 2 * w3 - 1, 20, sut.BackgroundBrush.Color);
            sut.WaveformImage.RectShouldHaveColor(2 * w3, 0, 3 * w3, 20, color);
            // 2/3
            e = new PeaksReceivedEvent(uri, 1, 2, peaks);
            sut.HandlePeaks(e);
            sut.WaveformImage.RectShouldHaveColor(0, 0, 2 * w3 - 1, 20, color);
            sut.WaveformImage.RectShouldHaveColor(2 * w3, 0, 3 * w3, 20, sut.BackgroundBrush.Color);
        }


        [Test]
        public void Handle_shift_events()
        {
            var ctx = new ContextFor<WaveformDisplayViewModel>();
            var sut = ctx.BuildSut();
            sut.Should().BeAssignableTo<IHandle<AudioShiftedEvent>>();

            var source = new Uri("http://some/audio/");
            var shift = TimeSpan.FromSeconds(3);
            var newStartTime =DateTimeOffset.Now;
            var e = new AudioShiftedEvent(source, shift, newStartTime);
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

            sut.IsLive.Should().BeFalse("innitially not live");
            sut.LastWritePosition.Should().Be(0);

            var peaks = new[]{ new PeakInfo(-1f,1f) };
            var uri = new Uri("channel://1/");
            var e = new PeaksReceivedEvent(uri, 0, 1, peaks);
            sut.Duration = e.End * 2;
            sut.HandlePeaks(e);
            sut.IsLive.Should().BeTrue("Receiving partial peaks should activate live tracking");
            sut.LastWritePosition.Should().Be(e.End, "last write position should be updated for received peaks");

            sut.HandleShift(sut.LastWritePosition);
            sut.LastWritePosition.Should().Be(0);
        }

        [Test]
        public void Cleanup_on_dispose()
        {
            var ctx = new ContextFor<WaveformDisplayViewModel>();
            var events = ctx.For<IEventAggregator>();

            WaveformDisplayViewModel sut;
            using (sut = ctx.BuildSut())
            {
                sut.WaveformImage.Should().NotBeNull();
                events.Received().Subscribe(sut);
            }

            events.Received().Unsubscribe(sut);
            sut.WaveformImage.Should().BeNull();
        }
    }
}
