using System.ComponentModel;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using FluentAssertions;
using NEdifis;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;
using NWaveform.Events;
using NWaveform.Interfaces;

namespace NWaveform.ViewModels
{
    [TestFixtureFor(typeof (WaveformViewModel))]
    // ReSharper disable InconsistentNaming
    internal class WaveformViewModel_Should
    {
        [Test(Description = "verify duration change on player is handles because of media stream")]
        public void Handle_Position_Changed_Event_From_MediaPlayer_If_Changed()
        {
            var positionPovider = Substitute.For<IMediaPlayer>();
            positionPovider.Duration.ReturnsForAnyArgs(2);

            var sut = new WaveformViewModel(positionPovider);
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

            var sut = new WaveformViewModel(positionPovider);
            sut.MonitorEvents();

            positionPovider.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(), new PropertyChangedEventArgs(nameof(sut.Duration)));
            sut.ShouldRaise(nameof(sut.PropertyChanged)).WithArgs<PropertyChangedEventArgs>(t => t.PropertyName == nameof(sut.HasDuration));
            sut.ShouldRaise(nameof(sut.PropertyChanged)).WithArgs<PropertyChangedEventArgs>(t => t.PropertyName == nameof(sut.Duration));
            sut.Duration.Should().Be(expectedDuration);
            sut.HasDuration.Should().BeTrue();
        }

        [Test(Description = "verify duration change on player is handles because of media stream")]
        public void Not_Handle_Duration_Changed_Event_From_MediaPlayer_If_Not_Changed()
        {
            const int expectedDuration = 4;
            var positionPovider = Substitute.For<IMediaPlayer>();
            positionPovider.Duration.ReturnsForAnyArgs(4);

            var sut = new WaveformViewModel(positionPovider) { Duration = 4 };
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
            var sut = new WaveformViewModel(positionPovider);

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

            sut.Should().BeAssignableTo<IHandle<SamplesReceivedEvent>>();
            sut.Duration = 2.0;

            var e = new SamplesReceivedEvent(0, 2, new []{ 1f, 1f, 0f});
            sut.Handle(e);

            var b = sut.WaveformImage;
            var w2 = (int) (b.Width / 2);
            var h2 = (int)(b.Height / 2);

            var color = b.GetPixel(1, 1);
            color.Should().Be(sut.LeftBrush.Color);

            color = b.GetPixel(w2-1, h2-1);
            color.Should().Be(sut.LeftBrush.Color);

            color = b.GetPixel(2*w2 - 1, h2 - 1);
            color.Should().Be(sut.BackgroundBrush.Color);
        }
    }
}
