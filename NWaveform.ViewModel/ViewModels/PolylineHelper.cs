using System.Collections.Generic;
using System.Linq;
using System.Windows;

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

            if (samples != null)
            {
                var maxX = samples.Count - 1;
                var maxY = samples.Max();
                if (maxX > 0 && maxY > double.Epsilon)
                {
                    var sX = 1.0 / maxX;
                    var sY = 1.0 / maxY;
                    for (int i = 0; i < samples.Count; i++)
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
    }
}