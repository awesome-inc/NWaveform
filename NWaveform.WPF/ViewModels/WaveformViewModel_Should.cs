using System;
using System.ComponentModel;
using System.Windows.Media;
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

            using (var monitor = sut.Monitor())
            {
                positionPovider.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(),
                    new PropertyChangedEventArgs(nameof(sut.Position)));

                monitor.Should().Raise(nameof(sut.PropertyChanged))
                    .WithArgs<PropertyChangedEventArgs>(t => t.PropertyName == nameof(sut.Position));
            }
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

            using (var monitor = sut.Monitor())
            {
                positionPovider.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(),
                    new PropertyChangedEventArgs(nameof(sut.Duration)));
                monitor.Should().Raise(nameof(sut.PropertyChanged))
                    .WithArgs<PropertyChangedEventArgs>(t => t.PropertyName == nameof(sut.HasDuration));
                monitor.Should().Raise(nameof(sut.PropertyChanged))
                    .WithArgs<PropertyChangedEventArgs>(t => t.PropertyName == nameof(sut.Duration));
                sut.Duration.Should().Be(expectedDuration);
                sut.HasDuration.Should().BeTrue();
            }
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

            using (var monitor = sut.Monitor())
            {
                positionPovider.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(),
                    new PropertyChangedEventArgs(nameof(sut.Duration)));
                monitor.Should().NotRaise(nameof(sut.PropertyChanged));
                monitor.Should().NotRaise(nameof(sut.PropertyChanged));
                sut.Duration.Should().Be(expectedDuration);
                sut.HasDuration.Should().BeTrue();
            }
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

            sut.Position.Should().Be(42);

            // ReSharper disable once RedundantAssignment // needed to match NSubstitute syntax!
            position = positionPovider.Received().Position;

            position = 48.0;
            sut.Position = position;
            positionPovider.Received().Position = position;
        }

        [Test]
        public void Use_Providers_Source()
        {
            var positionPovider = Substitute.For<IMediaPlayer>();
            var source = new Uri("my://source/");
            positionPovider.Source.Returns(source);

            var ctx = new ContextFor<WaveformViewModel>();

            var sut = ctx.BuildSut();

            // 1. test update on setting positionprovider
            sut.PositionProvider = positionPovider;
            sut.Source.Should().Be(source);
            // ReSharper disable once RedundantAssignment // needed to match NSubstitute syntax!
            source = positionPovider.Received(1).Source;

            // 2. test update on setting positionprovider.Source
            source = new Uri("other://source/");
            positionPovider.Source = source;
            positionPovider.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this,
                new PropertyChangedEventArgs(nameof(IPositionProvider.Source)));
            sut.Source.Should().Be(source);
            // ReSharper disable once RedundantAssignment // needed to match NSubstitute syntax!
            source = positionPovider.Received(2).Source;
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

            using (var monitor = sut.Monitor())
            {
                sut.HandleShift(3);

                monitor.Should().RaisePropertyChangeFor(x => x.Position,
                    "sut should not change position, just notify on the base stream (position provider)");
                sut.Selection.Start.Should().Be(3);
                sut.Selection.End.Should().Be(6);

                // TODO: assert channels & labels
            }
        }

        [Test]
        public void Support_auto_play()
        {
            var ctx = new ContextFor<WaveformViewModel>();
            var sut = ctx.BuildSut();

            var uri = new Uri("channel://1/");

            // set player
            var player = Substitute.For<IMediaPlayer>();
            player.Source = uri;
            sut.PositionProvider = player;
            player.CanPlay.Returns(true);

            // activate
            ((IActivate)sut).Activate();
            sut.IsActive.Should().BeTrue();

            // now, after receiving partial peaks, should start autplaying
            var peaks = new[] { new PeakInfo(-1f, 1f) };
            var e = new PeaksReceivedEvent(uri, 0, 1, peaks);
            sut.Duration = e.End * 2;
            sut.HandlePeaks(e);
            player.Received().Play();
            sut.Position.Should().Be(sut.LastWritePosition - sut.LiveDelta);

            player.ClearReceivedCalls();
            sut.HandlePeaks(e);
            // only play once after activation
            player.DidNotReceive().Play();

            // deactivate/activate & check autoplay again
            ((IDeactivate)sut).Deactivate(false);
            ((IActivate)sut).Activate();
            sut.HandlePeaks(e);
            player.Received().Play();
            sut.Position.Should().Be(sut.LastWritePosition - sut.LiveDelta);
        }

        [Test]
        public void Notify_property_changes()
        {
            var ctx = new ContextFor<WaveformViewModel>();
            var sut = ctx.BuildSut();

            using (var monitor = sut.Monitor())
            {
                var selection = new AudioSelectionViewModel();
                sut.Selection = selection;
                sut.Selection.Should().BeEquivalentTo(selection);
                monitor.Should().RaisePropertyChangeFor(x => x.Selection);
            }

            using (var monitor = sut.Monitor())
            {
                var selectionMenu = Substitute.For<IMenuViewModel>();
                sut.SelectionMenu = selectionMenu;
                sut.SelectionMenu.Should().Be(selectionMenu);
                monitor.Should().RaisePropertyChangeFor(x => x.SelectionMenu);
            }

            using (var monitor = sut.Monitor())
            {
                sut.TicksEach = 42.0;
                sut.TicksEach.Should().Be(42);
                monitor.Should().RaisePropertyChangeFor(x => x.TicksEach);
            }

            var brush = new SolidColorBrush(Colors.Red);
            var points = new PointCollection();
            var label = Substitute.For<ILabelVievModel>();
            var labels = new[] { label };
            sut.Labels = labels;
            sut.Labels.Should().BeEquivalentTo(labels);

            using (var monitor = sut.Monitor())
            {
                sut.SelectedLabel = label;
                sut.SelectedLabel.Should().Be(label);
                monitor.Should().RaisePropertyChangeFor(x => x.SelectedLabel);
            }

            using (var monitor = sut.Monitor())
            {
                sut.UserBrush = brush;
                sut.UserBrush.Should().Be(brush);
                monitor.Should().RaisePropertyChangeFor(x => x.UserBrush);
            }

            using (var monitor = sut.Monitor())
            {
                sut.SeparationLeftBrush = brush;
                sut.SeparationLeftBrush.Should().Be(brush);
                monitor.Should().RaisePropertyChangeFor(x => x.SeparationLeftBrush);
            }

            using (var monitor = sut.Monitor())
            {
                sut.SeparationRightBrush = brush;
                sut.SeparationRightBrush.Should().Be(brush);
                monitor.Should().RaisePropertyChangeFor(x => x.SeparationRightBrush);
            }

            using (var monitor = sut.Monitor())
            {
                sut.UserTextBrush = brush;
                sut.UserTextBrush.Should().Be(brush);
                monitor.Should().RaisePropertyChangeFor(x => x.UserTextBrush);
            }

            using (var monitor = sut.Monitor())
            {
                sut.PositionBrush = brush;
                sut.PositionBrush.Should().Be(brush);
                monitor.Should().RaisePropertyChangeFor(x => x.PositionBrush);
            }

            using (var monitor = sut.Monitor())
            {
                sut.SelectionBrush = brush;
                sut.SelectionBrush.Should().Be(brush);
                monitor.Should().RaisePropertyChangeFor(x => x.SelectionBrush);
            }

            using (var monitor = sut.Monitor())
            {
                sut.SeparationRightBrush = brush;
                sut.SeparationRightBrush.Should().Be(brush);
                monitor.Should().RaisePropertyChangeFor(x => x.SeparationRightBrush);
            }

            using (var monitor = sut.Monitor())
            {
                sut.UserChannel = points;
                sut.UserChannel.Should().BeEquivalentTo(points);
                monitor.Should().RaisePropertyChangeFor(x => x.UserChannel);
            }

            using (var monitor = sut.Monitor())
            {
                sut.SeparationLeftChannel = points;
                sut.SeparationLeftChannel.Should().BeEquivalentTo(points);
                monitor.Should().RaisePropertyChangeFor(x => x.SeparationLeftChannel);
            }

            using (var monitor = sut.Monitor())
            {
                sut.SeparationRightChannel = points;
                sut.SeparationRightChannel.Should().BeEquivalentTo(points);
                monitor.Should().RaisePropertyChangeFor(x => x.SeparationRightChannel);
            }
        }

        [Test]
        public void Cleanup()
        {
            var ctx = new ContextFor<WaveformViewModel>();
            WaveformViewModel sut;

            var events = ctx.For<IEventAggregator>();
            using (sut = ctx.BuildSut())
                events.Received().Subscribe(sut);
            events.Received().Unsubscribe(sut);
        }

        [Test]
        public void Set_Source_on_Selection()
        {
            var ctx = new ContextFor<WaveformViewModel>();
            var sut = ctx.BuildSut();
            sut.Selection.Source.Should().BeNull();

            var source = new Uri("my://source/");

            sut.Source = source;
            sut.Selection.Source.Should().Be(source);

            sut.Source = null;
            sut.Selection.Source.Should().BeNull();
        }
    }
}
