using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NWaveform.Exceptions;
using NWaveform.Interfaces;

namespace NWaveform.Default
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    internal class WindowsMediaPlayer_Should
    {
        [Test]
        [Explicit]
        public void Play_A_Music_File()
        {
            var file = new FileInfo(".\\Tests\\Pulp_Fiction_Jimmys_Coffee.mp3");
            file.Exists.Should().BeTrue();

            IMediaPlayer player = new WindowsMediaPlayer();
            player.Source = new Uri(file.FullName);

            player.Play();
            player.IsPlaying.Should().BeTrue();

            Thread.Sleep(1000);
            player.Stop();
        }

        [Test]
        [Explicit]
        public void Play_And_Pause_A_Music_File()
        {
            var file = new FileInfo(".\\Tests\\Pulp_Fiction_Jimmys_Coffee.mp3");
            file.Exists.Should().BeTrue();

            IMediaPlayer player = new WindowsMediaPlayer();

            player.Source = new Uri(file.FullName);

            player.Play();

            Thread.Sleep(2000);

            player.Pause();

            Thread.Sleep(2000);

            player.Play();

            Thread.Sleep(3000);

            player.Stop();
        }

        [Test, Explicit]
        public void Play_Triggers_An_Event()
        {
            var file = new FileInfo(".\\Tests\\Pulp_Fiction_Jimmys_Coffee.mp3");
            file.Exists.Should().BeTrue();

            IMediaPlayer player = new WindowsMediaPlayer();
            player.MonitorEvents();
            player.Source = new Uri(file.FullName);

            player.Play();
            Thread.Sleep(5000);

            // does not work!
            //player.ShouldRaise("PositionChanged").WithSender(player).WithArgs<EventArgs>(e=> e.Equals(EventArgs.Empty))
            player.Stop();
        }

        [Test]
        public void Wrap_media_exceptions_into_AudioException()
        {
            IMediaPlayer player = new WindowsMediaPlayer();
            player.Error.HasException.Should().BeFalse();

            player.Source = new Uri(@"c:\does.not.exist.wav");

            player.Error.HasException.Should().BeTrue();
            player.Error.Exception.Should().BeOfType<AudioException>();
            player.Error.Exception.Message.Should().StartWith("Could not open audio");

            player.Source = null;
            player.Error.HasException.Should().BeFalse();
        }

        [Test]
        public void Notify_Mute_on_Volume_changed()
        {
            var sut = new WindowsMediaPlayer();

            var map = new Dictionary<string, int>();
            sut.PropertyChanged += (s, e) =>
            {
                if (map.ContainsKey(e.PropertyName)) map[e.PropertyName]++;
                else map[e.PropertyName] = 1;
            };

            var volume = sut.Volume;
            sut.Volume.Should().NotBe(0);
            sut.IsMuted.Should().BeFalse();
            sut.CanMute.Should().BeTrue();

            sut.Volume = 0;
            sut.IsMuted.Should().BeTrue();
            sut.CanMute.Should().BeFalse();

            sut.Volume = volume;

            map["CanMute"].Should().Be(2);
            map["CanUnMute"].Should().Be(2);
            map["IsMuted"].Should().Be(2);
        }
    }
}