using System.Collections.Generic;
using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.ViewModels
{
    [TestFixtureFor(typeof(AudioSelectionViewModel))]
    // ReSharper disable InconsistentNaming
    internal class AudioSelectionViewModel_Should
    {
        [Test]
        public void Duration_should_be_negative_if_End_less_than_Start()
        {
            var sut = new AudioSelectionViewModel { Start = 1, End = 0 };
            sut.Duration.Should().BeNegative();
            sut.Duration.Should().Be(sut.End - sut.Start);
        }

        [Test]
        public void Changing_Start_or_End_should_raise_DurationChanged()
        {
            var sut = new AudioSelectionViewModel();

            sut.Top.Should().Be(-1);
            sut.Height.Should().Be(2);

            var map = new Dictionary<string, int>();
            sut.PropertyChanged += (s, e) =>
            {
                if (map.ContainsKey(e.PropertyName))
                    map[e.PropertyName]++;
                else
                    map[e.PropertyName] = 1;
            };

            sut.Start = 1.0;
            map[nameof(sut.Start)].Should().Be(1);
            map[nameof(sut.Duration)].Should().Be(1);
            sut.Start.Should().Be(1.0);

            sut.End = 2.0;
            map[nameof(sut.End)].Should().Be(1);
            map[nameof(sut.Duration)].Should().Be(2);
            sut.End.Should().Be(2.0);

            sut.Top = 0.2;
            map[nameof(sut.Top)].Should().Be(1);
            sut.Top.Should().Be(0.2);

            sut.Height = 1.0;
            map[nameof(sut.Height)].Should().Be(1);
            sut.Height.Should().Be(1.0);

            var menu = new MenuViewModel();
            sut.Menu = menu;
            map["Menu"].Should().Be(1);
            sut.Menu.Should().Be(menu);

            sut.Menu = null;
            map["Menu"].Should().Be(2);
            sut.Menu.Should().Be(null);
        }
    }
}