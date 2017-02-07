using NSubstitute;
using NUnit.Framework;
using NWaveform.Interfaces;

namespace NWaveform.Extensions
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    internal class MediaPlayerExtensions_Should
    {
        [Test]
        public void TogglePlayPause()
        {
            var player = Substitute.For<IMediaPlayer>();
            player.IsPaused.Returns(false, true);
            player.IsPlaying.Returns(true, false);
            
            player.TogglePlayPause();
            player.Received(1).Pause();
            player.DidNotReceive().Play();

            player.ClearReceivedCalls();
            player.TogglePlayPause();
            player.Received(1).Play();
            player.DidNotReceive().Pause();
        }

        [Test]
        public void ToggleMute()
        {
            var player = Substitute.For<IMediaPlayer>();
            player.IsMuted.Returns(false, true);

            player.ToggleMute();
            player.Received(1).Mute();
            player.DidNotReceive().UnMute();

            player.ClearReceivedCalls();
            player.ToggleMute();
            player.Received(1).UnMute();
            player.DidNotReceive().Mute();
        }
    }
}