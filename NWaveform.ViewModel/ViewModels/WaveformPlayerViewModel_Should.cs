using System;
using Caliburn.Micro;
using FluentAssertions;
using NEdifis;
using NSubstitute;
using NUnit.Framework;
using NWaveform.Extender;
using NWaveform.Interfaces;
using NWaveform.Model;

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
            var menu = new MenuViewModel(new[] {new MenuItemViewModel {Header = "test"}});
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
        public void Try_get_waveform_when_setting_Source()
        {
            var uri = new Uri("http://some/uri/audio.wav");
            var waveForm = new WaveformData
            {
                Duration = TimeSpan.FromSeconds(1),
                Channels = new[] { new Channel() }
            };

            var ctx = new ContextFor<WaveformPlayerViewModel>();
            ctx.For<IWaveFormRepository>().For(uri).Returns(waveForm);
            var sut = ctx.BuildSut();

            sut.Source = uri;

            sut.Waveforms.Received().For(uri);
            sut.Waveform.Received().SetWaveform(waveForm);
        }
    }
}