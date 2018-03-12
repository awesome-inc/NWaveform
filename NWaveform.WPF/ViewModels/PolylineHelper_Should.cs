using System.Linq;
using System.Windows;
using System.Windows.Media;
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

        [Test]
        public void Clamp()
        {
            var points = new[] {new Point(-0.1, -1.1), new Point(1.1, 2.1)};
            var clamped = points.Clamped(0, 1, -1, 2);
            clamped.Should().Equal(new Point(0, -1), new Point(1, 2));
        }

        [Test]
        public void Shift()
        {
            const double shift = 0.4, duration = 1.0;
            var points = new[]
            {
                new Point(shift - 0.2, 0), new Point(shift - 0.2, 1), new Point(shift - 0.1, 1),
                new Point(shift - 0.1, 0),
                new Point(shift - 0.1, 0), new Point(shift - 0.1, 1), new Point(shift + 0.1, 1),
                new Point(shift + 0.1, 0)
            };

            var shifted = new PointCollection(points).Shifted(shift, duration).ToArray();
            var expected = new[] {new Point(0, 0), new Point(0, 1), new Point(0.1, 1), new Point(0.1, 0)};
            
            //shifted.Should().Equal(expected);
            // NOTE: numerical issues (IEEE 754)
            shifted.Should().HaveCount(expected.Length);
            const double epsilon = 0.00000001;
            for (int i = 0; i < expected.Length; i++)
            {
                shifted[i].X.Should().BeApproximately(expected[i].X, epsilon);
                shifted[i].Y.Should().BeApproximately(expected[i].Y, epsilon);
            }
        }
    }
}
