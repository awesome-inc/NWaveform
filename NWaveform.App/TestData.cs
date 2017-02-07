using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using FontAwesome.Sharp;
using NWaveform.ViewModels;

namespace NWaveform.App
{
    internal static class TestData
    {
        public static IList<ILabelVievModel> GetRandomLabels(Random r, double duration, IMenuViewModel menu)
        {
            var labels = new List<ILabelVievModel>
            {
                new LabelVievModel
                {
                    Position = 15, Magnitude = -0.5, 
                    Text="Speaker A",
                    Tooltip = "Recognition Confidence: 80%",
                    Icon = IconChar.User, //IconChar.Male, IconChar.Child
                    Background = new SolidColorBrush(Colors.Red) { Opacity = 0.5},
                    Foreground = new SolidColorBrush(Colors.CornflowerBlue)
                },

                new LabelVievModel
                {
                    Position = 3, Magnitude = 0.5, 
                    Text="Location",
                    Tooltip = "Compute from 3 rays with 95% confidence",
                    Icon = IconChar.Flag, //IconChar.Crosshairs,
                    Background = new SolidColorBrush(Colors.Yellow) { Opacity = 0.5},
                    Foreground = new SolidColorBrush(Colors.DarkGreen)
                },

                new LabelVievModel
                {
                    Position = 56, Magnitude = -0.8, 
                    Text="Heading 78.4°",
                    Tooltip = "From North\r\n@footpoint: 73N74E ",
                    Icon = IconChar.LocationArrow,
                    Background = new SolidColorBrush(Colors.Green) { Opacity = 0.5},
                    Foreground = new SolidColorBrush(Colors.DarkMagenta)
                }
            };

            // randomly place labels
            foreach (var label in labels)
            {
                label.Position = (r.NextDouble() * 0.9 + 0.05) * duration;
                label.Magnitude = r.NextDouble() * 1.8 - 0.9;

                label.Background = new SolidColorBrush(RandomColor(r)) { Opacity = r.NextDouble() * 0.4 + 0.6 };
                label.Foreground = new SolidColorBrush(RandomColor(r));

                label.Menu = menu;
            }

            return labels;
        }

        public static IList<ILabelVievModel> GetWaypointsWithin(Random r, int n, IAudioSelectionViewModel selection, MenuViewModel waypointsMenu)
        {
            return Enumerable.Range(0, n).Select(i => (ILabelVievModel)new WaypointLabelViewModel(RandomWaypoint(r))
            {
                Position = (r.NextDouble() * selection.Duration + selection.Start),
                Menu = waypointsMenu
            }).ToList();
        }

        private static Color RandomColor(Random r)
        {
            return Color.FromArgb(255, (byte)r.Next(0, 255), (byte)r.Next(0, 255), (byte)r.Next(0, 255));
        }

        private static Waypoint RandomWaypoint(Random r)
        {
            return new Waypoint(r.NextDouble() * 359.0, RandomLocation(r));
        }

        private static Location RandomLocation(Random r)
        {
            return new Location(r.NextDouble() * 84 - 84, r.NextDouble() * 180 - 179);
        }

        public static PointCollection GetRandomWaypoints(Random r, double duration, int n = 0,
            double probWaypoint = 0.8, double yMin = 0.92, double yMax = 1.0, double wayPointWidth = 0.1)
        {
            var numPoints = n > 0 ? n : (int)duration; // ~1 waypoints/sec.

            var points = new List<Point>();
            for (int i = 0; i < numPoints; i++)
            {
                // dice for a waypoint/peak
                var d = r.NextDouble();
                var isWaypoint = d < probWaypoint;

                if (!isWaypoint) continue;

                var x = (i * duration / n) + d;
                x -= wayPointWidth;
                points.Add(new Point(x, yMax));
                points.Add(new Point(x, yMin));

                x += 2 * wayPointWidth;
                points.Add(new Point(x, yMin));
                points.Add(new Point(x, yMax));
            }

            return new PointCollection(points);
        }

        public static PointCollection GetSeparationTop(Random r, double duration, int n)
        {
            return GetSeparation(r, duration, n, -0.1, 0);
        }

        public static PointCollection GetSeparationBottom(Random r, double duration, int n)
        {
            return GetSeparation(r, duration, n, 0, 0.1);
        }

        private static PointCollection GetSeparation(Random r, double duration, int n, double top, double bottom)
        {
            if (n < 1) throw new ArgumentException("at least a split please");

            var splits = new[] { 0.0, duration }.Concat(Enumerable
                    .Range(0, n * 2)
                    .Select(x => r.NextDouble() * duration))
                .OrderBy(x => x).ToArray();

            var points = new List<Point>();
            for (var i = 1; i < splits.Length; i += 2)
            {
                points.Add(new Point(splits[i - 1], top));
                points.Add(new Point(splits[i - 1], bottom));

                points.Add(new Point(splits[i], bottom));
                points.Add(new Point(splits[i], top));
            }

            return new PointCollection(points);
        }
    }
}