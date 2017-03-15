using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    [TestFixtureFor(typeof(WaveformViewModel))]
    // ReSharper disable InconsistentNaming
    internal class WaveformViewModel_Should
    {
        [Test(Description = "verify duration change on player is handles because of media stream")]
        public void Handle_Position_Changed_Event_From_MediaPlayer_If_Changed()
        {
            var positionPovider = Substitute.For<IMediaPlayer>();
            positionPovider.Duration.ReturnsForAnyArgs(2);

            var ctx = new ContextFor<WaveformViewModel>();
            var sut = ctx.BuildSut();
            sut.PositionProvider = positionPovider;
            sut.MonitorEvents();

            positionPovider.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(), new PropertyChangedEventArgs(nameof(sut.Position)));
            sut.ShouldRaise(nameof(sut.PropertyChanged)).WithArgs<PropertyChangedEventArgs>(t => t.PropertyName == nameof(sut.Position));
        }

        [Test(Description = "verify duration change on player is handles because of media stream")]
        public void Handle_Duration_Changed_Event_From_MediaPlayer_If_Changed()
        {
            const int expectedDuration = 3;
            var positionPovider = Substitute.For<IMediaPlayer>();
            positionPovider.Duration.ReturnsForAnyArgs(expectedDuration);

            var ctx = new ContextFor<WaveformViewModel>();
            var sut = ctx.BuildSut();
            sut.PositionProvider = positionPovider;
            sut.MonitorEvents();

            positionPovider.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(), new PropertyChangedEventArgs(nameof(sut.Duration)));
            sut.ShouldRaise(nameof(sut.PropertyChanged)).WithArgs<PropertyChangedEventArgs>(t => t.PropertyName == nameof(sut.HasDuration));
            sut.ShouldRaise(nameof(sut.PropertyChanged)).WithArgs<PropertyChangedEventArgs>(t => t.PropertyName == nameof(sut.Duration));
            sut.Duration.Should().Be(expectedDuration);
            sut.HasDuration.Should().BeTrue();
        }

        [Test]
        public void Not_Handle_Duration_Changed_Event_From_MediaPlayer_If_Not_Changed()
        {
            const int expectedDuration = 4;
            var positionPovider = Substitute.For<IMediaPlayer>();
            positionPovider.Duration.ReturnsForAnyArgs(4);

            var ctx = new ContextFor<WaveformViewModel>();
            var sut = ctx.BuildSut();
            sut.Duration = 4.0;
            sut.PositionProvider = positionPovider;
            sut.MonitorEvents();

            positionPovider.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(), new PropertyChangedEventArgs(nameof(sut.Duration)));
            sut.ShouldNotRaise(nameof(sut.PropertyChanged));
            sut.ShouldNotRaise(nameof(sut.PropertyChanged));
            sut.Duration.Should().Be(expectedDuration);
            sut.HasDuration.Should().BeTrue();
        }

        [Test]
        public void Use_Providers_Position()
        {
            var position = 42.0;
            var positionPovider = Substitute.For<IMediaPlayer>();
            positionPovider.Position.Returns(position);

            var ctx = new ContextFor<WaveformViewModel>();
            var sut = ctx.BuildSut();
            sut.PositionProvider = positionPovider;
            sut.MonitorEvents();

            sut.Position.Should().Be(42);

            // ReSharper disable once RedundantAssignment // needed to match NSubstitute syntax!
            position = positionPovider.Received().Position;

            position = 48.0;
            sut.Position = position;
            positionPovider.Received().Position = position;
        }

        [Test]
        public void Handle_live_sample_updates()
        {
            var ctx = new ContextFor<WaveformViewModel>();
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
                .Select(m => new PeakInfo(m-1f, m)).ToArray();
            var e = new PeaksReceivedEvent(new Uri("source://test/"), 0, 2, peaks);
            sut.PositionProvider.Source = e.Source;
            sut.Duration = e.End;
            sut.HandlePeaks(e);

            // assert the image rendered
            sut.WaveformImage.RectShouldHaveColor(1, 1, w2, h2, sut.LeftBrush.Color);
            sut.WaveformImage.RectShouldHaveColor(w2 + 1, h2 + 1, 2 * w2, 2 * h2, sut.RightBrush.Color);
            sut.WaveformImage.RectShouldHaveColor(1, h2 + 1, w2 - 1, 2*h2-1, sut.BackgroundBrush.Color);
            sut.WaveformImage.RectShouldHaveColor(w2 + 1, 1, 2*w2, h2, sut.BackgroundBrush.Color);

            // assert the points are filled
            sut.LeftChannel.Take(10).ShouldAllBeEquivalentTo(0);
            sut.RightChannel.Take(10).ShouldAllBeEquivalentTo(h2);
            sut.LeftChannel.Skip(10).Take(10).ShouldAllBeEquivalentTo(h2);
            sut.RightChannel.Skip(10).Take(10).ShouldAllBeEquivalentTo(2*h2);
        }

        [Test]
        public void Only_Handle_events_for_same_source()
        {
            var ctx = new ContextFor<WaveformViewModel>();
            var sut = ctx.BuildSut();

            sut.BackgroundBrush = new SolidColorBrush(Colors.Black);
            sut.WaveformImage = BitmapFactory.New(20, 20);
            var e = new PeaksReceivedEvent(new Uri("source://test/"), 0, 1, new PeakInfo[0]);
            sut.PositionProvider.Source = new Uri("other://source/");
            sut.Handle(e);
            sut.WaveformImage.RectShouldHaveColor(0, 0, 20, 20, sut.BackgroundBrush.Color);
        }

        [Test]
        public void Refresh_waveform_when_receiving_earlier_samples_than_before()
        {
            var ctx = new ContextFor<WaveformViewModel>();
            var sut = ctx.BuildSut();

            sut.BackgroundBrush = new SolidColorBrush(Colors.Black);
            var color = Colors.Red;
            sut.LeftBrush = sut.RightBrush = new SolidColorBrush(color);
            sut.WaveformImage = BitmapFactory.New(30, 20);

            var uri = new Uri("source://test/");
            var w3 = (int)(sut.WaveformImage.Width / 3);
            var peaks = Enumerable.Repeat(1f, w3).Select(m => new PeakInfo(-m,m)).ToArray();

            sut.Duration = 3;
            // 1/3
            var e = new PeaksReceivedEvent(uri, 0, 1, peaks);
            sut.HandlePeaks(e);
            sut.WaveformImage.RectShouldHaveColor(0, 0, w3-1, 20, color);
            sut.WaveformImage.RectShouldHaveColor(w3, 0, 3*w3, 20, sut.BackgroundBrush.Color);
            // 3/3
            e = new PeaksReceivedEvent(uri, 2, 3, peaks);
            sut.HandlePeaks(e);
            sut.WaveformImage.RectShouldHaveColor(0, 0, w3 - 1, 20, color);
            sut.WaveformImage.RectShouldHaveColor(w3, 0, 2 * w3 -1, 20, sut.BackgroundBrush.Color);
            sut.WaveformImage.RectShouldHaveColor(2*w3, 0, 3 * w3, 20, color);
            // 2/3
            e = new PeaksReceivedEvent(uri, 1, 2, peaks);
            sut.HandlePeaks(e);
            sut.WaveformImage.RectShouldHaveColor(0, 0, 2 * w3 - 1, 20, color);
            sut.WaveformImage.RectShouldHaveColor(2 * w3, 0, 3 * w3, 20, sut.BackgroundBrush.Color);
        }

        [Test]
        public void Shift_position_selection_channels_and_labels()
        {
            var ctx = new ContextFor<WaveformViewModel>();
            var sut = ctx.BuildSut();

            sut.Duration = 12;
            sut.Position = 6;
            sut.Selection.Start = 6;
            sut.Selection.End = 9;

            sut.MonitorEvents();
            sut.HandleShift(3);

            sut.ShouldRaisePropertyChangeFor(x => x.Position, "sut should not change position, just notify on the base stream (position provider)");
            sut.Selection.Start.Should().Be(3);
            sut.Selection.End.Should().Be(6);

            // TODO: assert channels & labels
        }
    }
}
