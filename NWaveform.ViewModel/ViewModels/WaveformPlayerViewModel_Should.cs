using System;
using FluentAssertions;
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
            var context = new CreationContext();
            var menu = new MenuViewModel(new[] {new MenuItemViewModel {Header = "test"}});
            context.SelectionMenuProvider.Menu.Returns(menu);

            var sut = context.Create();

            sut.Waveform.SelectionMenu.Should().Be(menu);
        }

        [Test]
        public void Return_players_Source_when_getting_source()
        {
            var uri = new Uri("http://some/uri/audio.wav");
            var context = new CreationContext();
            context.Player.Source.Returns(uri);
            var sut = context.Create();
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

            var context = new CreationContext();
            context.Waveforms.For(uri).Returns(waveForm);
            var sut = context.Create();

            sut.Source = uri;

            sut.Waveforms.Received().For(uri);
            sut.Waveform.Received().SetWaveform(waveForm);
        }

        private class CreationContext
        {
            public IMediaPlayer Player { get; set; }
            public IWaveFormRepository Waveforms { get; set; }
            public IWaveformViewModel Waveform { get; set; }
            public IAudioSelectionMenuProvider SelectionMenuProvider { get; set; }

            public CreationContext()
            {
                Player = Substitute.For<IMediaPlayer>();
                Waveforms = Substitute.For<IWaveFormRepository>();
                Waveform = Substitute.For<IWaveformViewModel>();
                SelectionMenuProvider = Substitute.For<IAudioSelectionMenuProvider>();
            }

            public WaveformPlayerViewModel Create()
            {
                return new WaveformPlayerViewModel(Player, Waveforms, Waveform, SelectionMenuProvider);
            }
        }
    }
}