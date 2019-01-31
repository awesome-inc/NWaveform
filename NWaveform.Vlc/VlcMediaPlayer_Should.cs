using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Declarations;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using FluentAssertions;
using NAudio.CoreAudioApi;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;
using NWaveform.Exceptions;
using NWaveform.Model;
using NWaveform.NAudio;

namespace NWaveform.Vlc
{
    [TestFixtureFor(typeof(VlcMediaPlayer))]
    // ReSharper disable InconsistentNaming
    internal class VlcMediaPlayer_Should
    {
        #region CreationContext

        private static readonly Uri DefaultSource = new Uri(new FileInfo(Path.Combine(GetDirectory(Assembly.GetExecutingAssembly()), "StarWars_Dificult.mp3")).FullName);

        private static string GetDirectory(Assembly assembly)
        {
            var codeBase = assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var assemblyFile = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(assemblyFile);
        }

        private static VlcMediaPlayer CreateSut()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            return sut;
        }

        private class CreationContext
        {
            public IVideoPlayer Player { get; }
            public IMediaPlayerFactory Factory { get; }
            public AudioEndpointVolume VolumeEndpoint { get; set; }
            public Func<string, IMedia> CreateMedia { get; }

            public CreationContext()
            {
                // mock player and player events calls after async method calls
                Player = Substitute.For<IVideoPlayer>();
                Player.PlaybackRate.Returns(info => 1.0F);
                Player.When(player => player.Stop()).Do(info => Player.Events.PlayerStopped += Raise.Event());
                Player.When(player => player.Play()).Do(info => Player.Events.PlayerPlaying += Raise.Event());
                Player.When(player => player.Pause()).Do(info => Player.Events.PlayerPaused += Raise.Event());

                // init factory
                Factory = Substitute.For<IMediaPlayerFactory>();

                // init media factory
                CreateMedia = s =>
                {
                    var result = Substitute.For<IMedia>();
                    result.IsParsed.ReturnsForAnyArgs(info => true);
                    return result;
                };

                // let the factory return the proper factories (because they can be overwritten during test)
                Factory.CreatePlayer<IVideoPlayer>().ReturnsForAnyArgs(Player);
                Factory.CreateMedia<IMedia>(Arg.Any<string>()).ReturnsForAnyArgs(info => CreateMedia((string)info[0]));

                VolumeEndpoint = null;
            }

            public VlcMediaPlayer Create()
            {
                return new VlcMediaPlayer(Factory, VolumeEndpoint)
                {
                    Async = false,
                    Source = DefaultSource,
                };
            }
        }

        #endregion

        #region test exception handling

        [Test]
        public void Wrap_media_exceptions_into_AudioException()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Error.HasException.Should().BeFalse();

            sut.Source = new Uri(new FileInfo(@"c:\does.not.exist.wav").FullName);

            sut.Error.HasException.Should().BeTrue();
            sut.Error.Exception.Should().BeAssignableTo<AudioException>();
            sut.Error.Exception.Message.Should().StartWith("Could not open audio");

            sut.Source = null;
            sut.Error.HasException.Should().BeFalse();
        }

        #endregion

        #region property changed

        [Test]
        public void Notify_Mute_on_Volume_changed()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Volume = 0.5;

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

        [Test(Description = "in case the player stops internally, the status properties state changed")]
        public void Raise_PropertyChanged_If_Stopped()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Position = 0;
            sut.Duration = 100;

            using (var monitor = sut.Monitor())
            {
                sut.Stop();
                ctx.Player.Events.PlayerStopped += Raise.Event();

                monitor.Should().RaisePropertyChangeFor(x => x.IsPaused);
                monitor.Should().RaisePropertyChangeFor(x => x.IsStopped);
                monitor.Should().RaisePropertyChangeFor(x => x.CanPause);
                monitor.Should().RaisePropertyChangeFor(x => x.CanStop);
                monitor.Should().RaisePropertyChangeFor(x => x.CanPause);
            }
        }

        [Test(Description = "in case the player pauses, the status properties state changed")]
        public void Raise_PropertyChanged_If_Paused()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Position = 0;
            sut.Duration = 100;

            sut.Error.Exception.Should().BeNull();
            ctx.Player.Events.PlayerPlaying += Raise.Event();
            sut.IsPlaying.Should().BeTrue();

            using (var monitor = sut.Monitor())
            {

                sut.Pause();
                ctx.Player.Events.PlayerPaused += Raise.Event();

                monitor.Should().RaisePropertyChangeFor(x => x.IsPaused);
                monitor.Should().RaisePropertyChangeFor(x => x.IsStopped);
                monitor.Should().RaisePropertyChangeFor(x => x.CanPause);
                monitor.Should().RaisePropertyChangeFor(x => x.CanStop);
                monitor.Should().RaisePropertyChangeFor(x => x.CanPause);
            }
        }

        [Test(Description = "in case the player played, the status properties state changed")]
        public void Raise_PropertyChanged_If_Played()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Position = 0;
            sut.Duration = 100;

            using (var monitor = sut.Monitor())
            {
                ctx.Player.Events.PlayerPlaying += Raise.Event();

                monitor.Should().RaisePropertyChangeFor(x => x.IsPaused);
                monitor.Should().RaisePropertyChangeFor(x => x.IsStopped);
                monitor.Should().RaisePropertyChangeFor(x => x.CanPause);
                monitor.Should().RaisePropertyChangeFor(x => x.CanStop);
                monitor.Should().RaisePropertyChangeFor(x => x.CanPause);
            }
        }

        #endregion

        #region speed rate tests

        [Test]
        public void Allow_SpeedChange_If_Normal_Speed()
        {
            var sut = CreateSut();

            sut.Rate = 1.0;
            sut.Rate.Should().Be(1);
            sut.Rate.Should().BeGreaterThan(sut.MinRate);
            sut.Rate.Should().BeLessThan(sut.MaxRate);

            sut.CanFaster.Should().BeTrue();
            sut.CanSlower.Should().BeTrue();
        }

        [Test]
        public void IncreaseSpeed()
        {
            var sut = CreateSut();

            sut.Rate = 1.0;
            sut.Rate.Should().Be(1);
            sut.CanFaster.Should().BeTrue();
            sut.Faster();
            sut.Rate.Should().Be(1 + sut.RateDelta);
        }

        [Test]
        public void IncreaseSpeed_With_Maximum()
        {
            var sut = CreateSut();

            var rate = sut.MaxRate - sut.RateDelta;
            sut.Rate = rate;

            sut.Rate.Should().Be(rate);
            sut.CanFaster.Should().BeTrue();
            sut.Faster();
            sut.Rate.Should().Be(sut.MaxRate);
            sut.CanFaster.Should().BeFalse();
        }

        [Test]
        public void DecreaseSpeed()
        {
            var sut = CreateSut();

            sut.Rate = 1.0;
            sut.Rate.Should().Be(1);
            sut.CanSlower.Should().BeTrue();
            sut.Slower();
            sut.Rate.Should().Be(1 - sut.RateDelta);
        }

        [Test]
        public void DecreaseSpeed_With_Minimum()
        {
            var sut = CreateSut();

            var rate = sut.MinRate + sut.RateDelta;
            sut.Rate = rate;
            sut.Rate.Should().Be(rate);
            sut.CanSlower.Should().BeTrue();
            sut.Slower();
            sut.Rate.Should().Be(sut.MinRate);
            sut.CanSlower.Should().BeFalse();
        }

        [Test]
        public void Not_Allow_SpeedChange_If_Min_Or_Max_Speed()
        {
            var sut = CreateSut();

            sut.Rate = sut.MaxRate;
            sut.CanFaster.Should().BeFalse();
            sut.CanSlower.Should().BeTrue();

            sut.Rate = sut.MinRate;
            sut.CanFaster.Should().BeTrue();
            sut.CanSlower.Should().BeFalse();
        }

        [Test]
        public void Not_Allow_SpeedChange_To_More_Than_Speed_Limits()
        {
            var sut = CreateSut();

            sut.Rate = 2 * sut.MaxRate;
            sut.Rate.Should().Be(sut.MaxRate);

            sut.Rate = 0.5 * sut.MinRate;
            sut.Rate.Should().Be(sut.MinRate);
        }

        #endregion

        #region duration/error tests

        [Test(Description = "")]
        public void Not_Change_Relative_Position_If_Duration_Changed_And_Playing()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Duration = 100;
            sut.Position = 50;

            ctx.Player.Events.PlayerPlaying += Raise.Event();
            ctx.Player.Events.PlayerLengthChanged += Raise.EventWith(new MediaPlayerLengthChanged(200000));

            sut.Duration.Should().Be(200);
            sut.Position.Should().Be(50);
        }

        [Test(Description = "")]
        public void Change_Relative_Position_If_Duration_Changed_If_Not_Playing()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Duration = 100;
            sut.Position = 50;

            ctx.Player.Events.PlayerLengthChanged += Raise.EventWith(new MediaPlayerLengthChanged(200000));

            sut.Duration.Should().Be(200);
            //sut.Position.Should().Be(25);
        }

        [Test(Description = "")]
        public void Transform_Vlc_Length_From_Milliseconds_to_Seconds()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();

            using (var monitor = sut.Monitor())
            {
                ctx.Player.Events.PlayerLengthChanged += Raise.EventWith(new MediaPlayerLengthChanged(15000));

                sut.Duration.Should().Be(15);
                monitor.Should().RaisePropertyChangeFor(player => player.Duration);
            }
        }

        [Test(Description = "")]
        public void Handle_Vlc_Error_With_Audio_Excpetion()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();

            sut.Error.Should().Be(AudioError.NoError);
            sut.Error.HasException.Should().BeFalse();

            ctx.Player.Events.PlayerEncounteredError += Raise.EventWith(new MediaPlayerLengthChanged(15000));

            sut.Error.Should().NotBeNull();
            sut.Error.HasException.Should().BeTrue();
            sut.Error.Should().BeOfType<AudioError>();
            sut.Error.Exception.Should().BeOfType<AudioException>();
        }

        #endregion

        #region Can* and Is* property verification

        [Test(Description = "")]
        public void Initialize_source_with_proper_state()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Position = 0;
            sut.Duration = 100;

            File.Exists(sut.Source.LocalPath).Should().BeTrue();
            sut.HasAudio.Should().BeTrue();

            sut.CanPlay.Should().BeTrue("we did nothing so far, so the user can hit play");
            sut.CanPause.Should().BeFalse();
            sut.CanStop.Should().BeFalse("because we ARE at the starting point");

            sut.IsPlaying.Should().BeFalse();
            sut.IsPaused.Should().BeFalse();
            sut.IsStopped.Should().BeTrue("<-- this is where we are");
        }

        [Test(Description = "")]
        public void Have_proper_state_after_hitting_play()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Position = 10;
            sut.Duration = 100;

            sut.PlayWithTask().Wait();

            File.Exists(sut.Source.LocalPath).Should().BeTrue();
            sut.HasAudio.Should().BeTrue();

            sut.CanPlay.Should().BeFalse();
            sut.CanPause.Should().BeTrue();
            sut.CanStop.Should().BeTrue();

            sut.IsPlaying.Should().BeTrue("<-- this is where we are");
            sut.IsPaused.Should().BeFalse();
            sut.IsStopped.Should().BeFalse();
        }

        [Test(Description = "")]
        public void Have_proper_state_after_hitting_play_pause()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Position = 10;
            sut.Duration = 100;

            File.Exists(sut.Source.LocalPath).Should().BeTrue();
            sut.HasAudio.Should().BeTrue();

            sut.PlayWithTask().Wait();
            sut.Position.Should().Be(10);
            sut.Pause();

            sut.CanPlay.Should().BeTrue("user can hit play again");
            sut.CanPause.Should().BeFalse();
            sut.CanStop.Should().BeTrue("we are NOT at the starting point");

            sut.IsPlaying.Should().BeFalse();
            sut.IsPaused.Should().BeTrue("<-- this is where we are");
            sut.IsStopped.Should().BeFalse();
        }

        [Test(Description = "")]
        public void Have_proper_state_after_hitting_play_stop()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Position = 10;
            sut.Duration = 100;

            File.Exists(sut.Source.LocalPath).Should().BeTrue();
            sut.HasAudio.Should().BeTrue();

            sut.PlayWithTask().Wait();
            sut.Stop();

            // not now: sut.Position.Should().Be(0);
            sut.CanPlay.Should().BeTrue("user can hit play again");
            sut.CanPause.Should().BeFalse();
            sut.CanStop.Should().BeFalse("we are at the starting point");

            sut.IsPlaying.Should().BeFalse();
            sut.IsPaused.Should().BeFalse();
            sut.IsStopped.Should().BeTrue("<-- this is where we are");
        }

        // after doing the easy stuff, we do some corner cases
        [Test(Description = "because at least the play button should be visible")]
        public void Allow_play_if_at_the_end()
        {
            var ctx = new CreationContext();
            ctx.Player.Length.Returns(100);
            var sut = ctx.Create();
            sut.Position = 10;
            sut.Duration = 100;

            File.Exists(sut.Source.LocalPath).Should().BeTrue();
            sut.HasAudio.Should().BeTrue();

            sut.PlayWithTask().Wait();
            ctx.Player.Events.PlayerPositionChanged += Raise.EventWith(new object(), new MediaPlayerPositionChanged(1));
            sut.Pause();

            sut.CanPlay.Should().BeTrue("user can hit play again");
            sut.CanPause.Should().BeFalse();
            sut.CanStop.Should().BeTrue("we are NOT at the starting point");

            sut.IsPlaying.Should().BeFalse();
            sut.IsPaused.Should().BeTrue("<-- this is where we are");
            sut.IsStopped.Should().BeFalse();
        }

        [Test(Description = "play-method should not be called if at he end of the audio")]
        public void Not_invoke_play_if_at_the_end()
        {
            var ctx = new CreationContext();
            ctx.Player.Length.Returns(100000);
            var sut = ctx.Create();
            sut.Position = 10;
            sut.Duration = 100;

            File.Exists(sut.Source.LocalPath).Should().BeTrue();
            sut.HasAudio.Should().BeTrue();

            sut.PlayWithTask().Wait();
            ctx.Player.Events.PlayerPositionChanged += Raise.EventWith(new object(), new MediaPlayerPositionChanged(100));
            sut.Pause();

            ctx.Player.ClearReceivedCalls();
            sut.PlayWithTask().Wait();
            ctx.Player.Received().Play();
        }

        [Test(Description = "automatically pause if at he end of the audio")]
        public void Automatically_pause_if_at_the_end()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            sut.Position = 10;
            sut.Duration = 100;

            File.Exists(sut.Source.LocalPath).Should().BeTrue();
            sut.HasAudio.Should().BeTrue();

            sut.PlayWithTask().Wait();
            ctx.Player.Events.MediaEnded += Raise.EventWith(new object(), EventArgs.Empty);

            sut.CanPlay.Should().BeTrue();
            sut.CanStop.Should().BeTrue();
            sut.CanPause.Should().BeFalse();

            sut.IsPaused.Should().BeTrue();
            sut.IsPlaying.Should().BeFalse();
            sut.IsStopped.Should().BeFalse();
        }

        [Test(Description = "#12360: always change volume")]
        public void Initialize_volume_on_setting_source()
        {
            var ctx = new CreationContext();
            var sut = ctx.Create();
            ctx.Player.Volume.Returns(-1);

            sut.Source = null;

            using (var monitor = sut.Monitor())
            {
                sut.Source = DefaultSource;

                monitor.Should().RaisePropertyChangeFor(p => p.Volume);
                monitor.Should().RaisePropertyChangeFor(p => p.CanMute);
                monitor.Should().RaisePropertyChangeFor(p => p.CanUnMute);

                monitor.Should().RaisePropertyChangeFor(p => p.CanFaster);
                monitor.Should().RaisePropertyChangeFor(p => p.CanSlower);
                monitor.Should().RaisePropertyChangeFor(p => p.CanPlay);
                monitor.Should().RaisePropertyChangeFor(p => p.CanPause);
                monitor.Should().RaisePropertyChangeFor(p => p.CanStop);
            }

            sut.Volume.Should().Be(VlcMediaPlayer.DefaultVolume);
            VlcMediaPlayer.DefaultVolume.Should().Be(1.0, "this seems to be VLCs default");

            VlcMediaPlayer.VolumeEps.Should().BeLessOrEqualTo(0.1, "At minimum support 10% volume changes");

            using (var monitor = sut.Monitor())
            {
                sut.Volume = 0.5;
                monitor.Should().RaisePropertyChangeFor(p => p.Volume);
                sut.Volume.Should().Be(0.5);
            }

            using (var monitor = sut.Monitor())
            {
                // check that updating player state/volume notifies the UI
                ctx.Player.Volume.Returns(10);
                sut.PlayWithTask().Wait();
                monitor.Should().RaisePropertyChangeFor(p => p.Volume);
                sut.Volume.Should().Be(0.5);

                sut.Volume = 0.2;
                sut.Volume.Should().Be(0.2);
            }

            using (var monitor = sut.Monitor())
            {
                // check that any play/pause changes notify on Volume
                sut.Pause();
                monitor.Should().RaisePropertyChangeFor(p => p.Volume);
            }

            using (var monitor = sut.Monitor())
            {
                sut.Stop();
                monitor.Should().RaisePropertyChangeFor(p => p.Volume);
            }
        }


        #endregion

        [Test(Description = "#12526 :Balance"), Explicit]
        public void Support_Balance()
        {
#if NCRUNCH
            Assert.Inconclusive("skip for NCrunch");
#endif

            var ctx = new CreationContext { VolumeEndpoint = CoreAudio.GetDefaultVolumeEndpoint() };

            var sut = ctx.Create();

            var duration = TimeSpan.FromSeconds(3);
            const int n = 50;

            var dt = (int)(duration.TotalMilliseconds / n);
            for (var i = 1; i <= n; i++)
            {
                var balance = Math.Sin(2 * i * Math.PI / n);
                sut.Balance = balance;

                Thread.Sleep(dt);

                sut.Balance.Should().BeApproximately(balance, VlcMediaPlayer.BalanceEps);
            }
        }
    }
}
