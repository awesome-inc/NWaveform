using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;
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
    [TestFixtureFor(typeof(WaveformViewModel))]
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

            // |----------         |
            // |-------------------|
            // |          ---------|   
            sut.WaveformImage = BitmapFactory.New(20, 20);
            sut.LeftBrush = new SolidColorBrush(Colors.Red);
            sut.RightBrush = new SolidColorBrush(Colors.Blue);
            sut.BackgroundBrush = new SolidColorBrush(Colors.Black);
            var w2 = (int)(sut.WaveformImage.Width / 2);
            var h2 = (int)(sut.WaveformImage.Height / 2);

            var samples = Enumerable.Repeat(1f, 2 * w2).Concat(Enumerable.Repeat(0f, 2 * w2)).ToArray();
            var e = new SamplesReceivedEvent(0, 2, samples, samples.Reverse().ToArray());
            sut.Duration = e.End;
            sut.Handle(e);

            RectShouldHaveColor(sut.WaveformImage, 1, 1, w2, h2, sut.LeftBrush.Color);
            RectShouldHaveColor(sut.WaveformImage, w2 + 1, h2 + 1, 2 * w2, 2 * h2, sut.RightBrush.Color);
            RectShouldHaveColor(sut.WaveformImage, 1, h2 + 1, w2 - 1, 2*h2-1, sut.BackgroundBrush.Color);
            RectShouldHaveColor(sut.WaveformImage, w2 + 1, 1, 2*w2, h2, sut.BackgroundBrush.Color);
        }

        private static unsafe void RectShouldHaveColor(WriteableBitmap b, int x0, int y0, int x1, int y1, Color color, [CallerMemberName]string test = null)
        {
            var expectedColor = WriteableBitmapExtensions.ConvertColor(color);
            var expectedColorName = GetColorName(color);
            using (var c = b.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                for (var y = y0; y < y1; y++)
                    for (var x = x0; x < x1; x++)
                    {
                        var actualColor = c.Pixels[y * c.Width + x];
                        if (actualColor != expectedColor)
                        {
                            var actualColorName = GetColorName(b.GetPixel(x,y));
                            Assert.Fail($"Pixel at ({x},{y}) should be '{expectedColorName}' but is '{actualColorName}'");
                        }
                    }
            }
        }

        private static string GetColorName(Color col)
        {
            var colorProperty = typeof(Colors).GetProperties()
                .FirstOrDefault(p => Color.AreClose((Color)p.GetValue(null), col));
            return colorProperty != null ? colorProperty.Name : col.ToString();
        }
    }
}
