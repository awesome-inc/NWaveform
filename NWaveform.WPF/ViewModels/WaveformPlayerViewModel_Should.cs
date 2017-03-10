using System;
using Caliburn.Micro;
using FluentAssertions;
using NEdifis;
using NSubstitute;
using NUnit.Framework;
using NWaveform.Events;
using NWaveform.Extender;
using NWaveform.Interfaces;

namespace NWaveform.ViewModels
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    internal class WaveformPlayerViewModel_Should
    {
        [Test]
        public void Initialize_selection_menu_on()
        {
            var ctx = new ContextFor<WaveformPlayerViewModel>();
            var menu = new MenuViewModel(new[] { new MenuItemViewModel { Header = "test" } });
            ctx.For<IAudioSelectionMenuProvider>().Menu.Returns(menu);

            var sut = ctx.BuildSut();

            sut.Waveform.SelectionMenu.Should().Be(menu);
        }

        [Test]
        public void Return_players_Source_when_getting_source()
        {
            var uri = new Uri("http://some/uri/audio.wav");
            var ctx = new ContextFor<WaveformPlayerViewModel>();
            ctx.For<IMediaPlayer>().Source.Returns(uri);
            var sut = ctx.BuildSut();
            sut.Source.Should().Be(uri);

        }

        [Test]
        public void Support_absolute_audio_time()
        {
            var ctx = new ContextFor<WaveformPlayerViewModel>();
            var formatter = new DateTimeFormatter("yyyy-MM-dd HH:mm:ss");
            ctx.Use<IAbsoluteTimeFormatter>(formatter);
            var sut = ctx.BuildSut();

            sut.HasCurrentTime.Should().BeFalse("no start time by default");
            sut.CurrentTime.Should().BeNullOrWhiteSpace();

            var d = DateTimeOffset.UtcNow;
            sut.StartTime = d;

            sut.HasCurrentTime.Should().BeTrue();

            var position = TimeSpan.FromSeconds(sut.Player.Duration);
            sut.Player.Position = position.TotalSeconds;

            var expected = formatter.Format(d + position);
            sut.CurrentTime.Should().Be(expected);
        }

        [Test]
        public void Support_start_time_with_shifts()
        {
            var uri = new Uri("http://some/uri/audio.wav");
            var ctx = new ContextFor<WaveformPlayerViewModel>();

            var d = DateTimeOffset.UtcNow;
            var sut = ctx.BuildSut();

            sut.Should().BeAssignableTo<IHandle<AudioShiftedEvent>>();
            ctx.For<IEventAggregator>().Received().Subscribe(sut);

            var getTime = ctx.For<IGetTimeStamp>();
            getTime.For(uri).Returns(d);

            sut.Source = uri;
            sut.HasCurrentTime.Should().BeTrue();
            sut.StartTime.Should().Be(d);

            var shiftedEvent = new AudioShiftedEvent(uri, TimeSpan.FromSeconds(3));
            sut.Handle(shiftedEvent);
            sut.StartTime.Should().Be(d + shiftedEvent.Shift);
        }
    }
}