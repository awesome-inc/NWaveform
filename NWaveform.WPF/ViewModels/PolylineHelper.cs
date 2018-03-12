using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NWaveform.ViewModels
{
    internal static class PolylineHelper
    {
        /// <summary>
        /// Returns a set of points in normalized [0,1] x [0,1]
        /// </summary>
        public static IList<Point> ToPoints(this IList<float> samples)
        {
            IList<Point> points = new List<Point> { new Point(0, 0) };

            if (samples != null && samples.Count > 1)
            {
                var maxX = samples.Count - 1;
                var maxY = samples.Max();
                if (maxY > double.Epsilon)
                {
                    var sX = 1.0 / maxX;
                    var sY = 1.0 / maxY;
                    for (var i = 0; i < samples.Count; i++)
                    {
                        var x = sX * i;
                        var y = sY * samples[i];
                        points.Add(new Point(x, y));
                    }
                }
            }

            points.Add(new Point(1, 0));

            return points;
        }

        public static IList<Point> Scaled(this IEnumerable<Point> points, double scaleX, double scaleY)
        {
            return points.Scaled(new Vector(scaleX, scaleY));
        }

        public static IList<Point> Scaled(this IEnumerable<Point> points, Vector scale)
        {
            return points.Select(p => p.Scaled(scale)).ToList();
        }

        public static Point Scaled(this Point point, Vector scale)
        {
            return new Point(scale.X * point.X, scale.Y * point.Y);
        }

        public static PointCollection Shifted(this PointCollection points, double shift, double duration)
        {

            if (points == null) return null;
            var shifted = points.Select(p => new Point(p.X - shift, p.Y))
                .ToList();

            // by now, always quadruples (boxes). Preserve that!
            var clipped = new List<Point>(shifted.Count);
            for (int i = 0; i < shifted.Count/4; i ++)
            {
                var q = shifted.Skip(i*4).Take(4).ToArray();
                if (q.All(p => p.X < 0 || p.X > duration)) continue;

                q = q.Clamped(0, duration).ToArray();

                clipped.AddRange(q);
            }

            return new PointCollection(clipped);
        }

        public static IList<Point> Clamped(this IEnumerable<Point> points, double minX, double maxX
            , double minY = double.NegativeInfinity, double maxY = double.PositiveInfinity)
        {
            var min = new Vector(minX, minY);
            var max = new Vector(maxX, maxY);
            return points.Clamped(min, max);
        }

        public static IList<Point> Clamped(this IEnumerable<Point> points, Vector min, Vector max)
        {
            return points.Select(p => p.Clamped(min, max)).ToList();
        }

        public static Point Clamped(this Point point, Vector min, Vector max)
        {
            return new Point(
                Math.Max(min.X, Math.Min(max.X, point.X)),
                Math.Max(min.Y, Math.Min(max.Y, point.Y)));
        }
    }
}
