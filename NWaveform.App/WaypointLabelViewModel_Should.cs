using FluentAssertions;
using FontAwesome.Sharp;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.App
{
    [TestFixtureFor(typeof(WaypointLabelViewModel))]
    // ReSharper disable InconsistentNaming
    internal class WaypointLabelViewModel_Should
    {
        [Test]
        public void Be_Creatable()
        {
            var reference = new Location(51,7);
            var waypoint = new Waypoint(0, reference);
            var sut = new WaypointLabelViewModel(waypoint);
            sut.Icon.Should().Be(IconChar.LocationArrow);
        }
    }
}