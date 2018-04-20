using Caliburn.Micro;
using FluentAssertions;
using NAudio.Wave;
using NEdifis;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;

namespace NWaveform.App
{
    [TestFixtureFor(typeof(ChannelsViewModel))]
    // ReSharper disable once InconsistentNaming
    internal class ChannelsViewModel_Should
    {
        [Test]
        public void Play_pause()
        {
            var ctx = new ContextFor<ChannelsViewModel>();
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8000, 1);
            var mixer = ctx.For<IChannelMixer>();
            mixer.SampleProvider.WaveFormat.Returns(waveFormat);
            var player = ctx.For<IWavePlayer>();
            player.When(x => x.Play()).Do(x => player.PlaybackState.Returns(PlaybackState.Playing));
            player.When(x => x.Pause()).Do(x => player.PlaybackState.Returns(PlaybackState.Paused));

            var sut = ctx.BuildSut();
            sut.IsPlaying.Should().Be(sut.IsActive);

            ((IActivate)sut).Activate();
            sut.IsPlaying.Should().Be(sut.IsActive);

            sut.Pause();
            sut.IsPlaying.Should().BeFalse();

            sut.Play();
            sut.IsPlaying.Should().BeTrue();

            ((IDeactivate)sut).Deactivate(false);
            sut.IsPlaying.Should().Be(sut.IsActive);
        }
    }
}
