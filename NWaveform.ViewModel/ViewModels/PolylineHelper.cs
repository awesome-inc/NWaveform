using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NWaveform.ViewModels
{
    /// <summary>
    /// Provides polyline simplification via vertex decimation using the Douglas-Peucker algorithm, 
    /// cf.: http://www.hpl.hp.com/techreports/Compaq-DEC/SRC-RR-133A.pdf)
    /// </summary>
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
                var sX = 1.0 / (samples.Count - 1);
                var sY = 1.0 / samples.Max();
                for (int i = 0; i < samples.Count; i++)
                {
                    var x = sX * i;
                    var y = sY * samples[i];
                    points.Add(new Point(x, y));
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

        public static IList<Point> Simplified(this IList<Point> verts, double maxError)
        {
            var indices = IndicesOfNonDecimatedVertices(verts, maxError);
            var simplified = new Point[indices.Length];
            for (var i = 0; i < indices.Length; i++)
            {
                simplified[i] = verts[indices[i]];
            }
            return simplified;
        }

        private static int[] IndicesOfNonDecimatedVertices(IList<Point> verts, double maxError)
        {
            if (verts.Count < 3)
                return new int[0];

            var surviving = new bool[verts.Count];
            surviving[0] = true;
            surviving[verts.Count - 1] = true;
            for (var i = 1; i < verts.Count - 1; i++)
                surviving[i] = false;

            DouglasPeucker(verts, surviving, 0, verts.Count - 1, maxError * maxError);

            // 1st pass: count surviving vertices
            var m = 0;
            for (var i = 0; i < verts.Count; i++)
            {
                if (surviving[i])
                    m++;
            }
            // 2nd pass: set surviving indices
            var indices = new int[m];
            m = 0;
            for (var i = 0; i < verts.Count; i++)
            {
                if (surviving[i])
                {
                    indices[m] = i;
                    m++;
                }
            }

            return indices;
        }

        private static void DouglasPeucker(IList<Point> verts, IList<bool> surviving,
            int start, int end, double maxErr2)
        {
            if (end - start < 2)
            {
                surviving[start] = true;
                surviving[end] = true;
                return;
            }

            var maxD2 = 0.0;
            var maxI = start;

            var v = verts[end] - verts[start];
            var c2 = v.LengthSquared;

            // special case for a (numerically) closed loop
            // Somewhat adhoc, just calculate euclidian distances to the crossing point
            if (c2 < double.Epsilon)
            {
                for (var i = start + 1; i < end; i++)
                {
                    var d2 = (verts[i] - verts[start]).LengthSquared;
                    if (d2 > maxD2)
                    {
                        maxD2 = d2;
                        maxI = i;
                    }
                }
            }
            else
            {
                for (var i = start + 1; i < end; i++)
                {
                    // compute distance to line segment
                    double d2;
                    var w = verts[i] - verts[start];

                    var c1 = v * w;
                    if (c1 <= 0.0)
                    {
                        // Shortest distance of line segment to the point is the euclidian distance
                        d2 = w.LengthSquared;
                    }
                    else
                    {
                        if (c2 <= c1)
                        {
                            // Shortest distance of line segment to the point is the euclidian distance
                            d2 = (verts[i] - verts[end]).LengthSquared;
                        }
                        else
                        {
                            // Shortest distance is orthogonal to the line
                            var b = c1 / c2;
                            var pb = verts[start] + b * v;
                            d2 = (verts[i] - pb).LengthSquared;
                        }
                    }

                    if (d2 > maxD2)
                    {
                        maxD2 = d2;
                        maxI = i;
                    }
                }
            }

            if (maxD2 > maxErr2)
            {
                surviving[maxI] = true;
                DouglasPeucker(verts, surviving, start, maxI, maxErr2);
                DouglasPeucker(verts, surviving, maxI, end, maxErr2);
            }
        }
    }
}