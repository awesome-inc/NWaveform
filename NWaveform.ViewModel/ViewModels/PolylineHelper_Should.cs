using System.Linq;
using System.Windows;
using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.ViewModels
{
    [TestFixtureFor(typeof(PolylineHelper))]
    // ReSharper disable InconsistentNaming
    internal class PolylineHelper_Should
    {
        [Test]
        public void Lerp_samples()
        {
            const int n = 5;
            var samples = Enumerable.Range(0, n).Select(i => i / (float) (n - 1)).ToList();
            var actual = samples.ToPoints().Skip(1).Take(n);
            var expected = samples.Select(f => new Point(f, f)).ToList();
            actual.Should().BeEquivalentTo(expected);

        }

        [Test]
        public void Skip_silent_samples()
        {
            var samples = Enumerable.Repeat(0f, 4).ToList();
            var actual = samples.ToPoints();
            actual.Should().BeEquivalentTo(new[] {new Point(0, 0), new Point(1, 0)});
        }

        [Test]
        public void Scale_samples()
        {
            var samples = new [] {1f, 1f};
            var points = samples.ToPoints().Skip(1).Take(2).ToList();
            var scale = new Vector(2,4);
            var actual = points.Scaled(scale.X, scale.Y);
            var expected = points.Select(p => new Point(p.X*scale.X, p.Y*scale.Y)).ToList();
            actual.Should().BeEquivalentTo(expected);
        }
    }
}