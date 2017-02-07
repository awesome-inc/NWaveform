using System.ComponentModel;
using FluentAssertions;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;
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

            positionPovider.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(), new PropertyChangedEventArgs("Position"));
            sut.ShouldRaise("PropertyChanged").WithArgs<PropertyChangedEventArgs>(t => t.PropertyName == "Position");
        }

        [Test(Description = "verify duration change on player is handles because of media stream")]
        public void Handle_Duration_Changed_Event_From_MediaPlayer_If_Changed()
        {
            const int expectedDuration = 3;
            var positionPovider = Substitute.For<IMediaPlayer>();
            positionPovider.Duration.ReturnsForAnyArgs(expectedDuration);

            var sut = new WaveformViewModel(positionPovider);
            sut.MonitorEvents();

            positionPovider.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(), new PropertyChangedEventArgs("Duration"));
            sut.ShouldRaise("PropertyChanged").WithArgs<PropertyChangedEventArgs>(t => t.PropertyName == "HasDuration");
            sut.ShouldRaise("PropertyChanged").WithArgs<PropertyChangedEventArgs>(t => t.PropertyName == "Duration");
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

            positionPovider.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(), new PropertyChangedEventArgs("Duration"));
            sut.ShouldNotRaise("PropertyChanged");
            sut.ShouldNotRaise("PropertyChanged");
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
    }
}
