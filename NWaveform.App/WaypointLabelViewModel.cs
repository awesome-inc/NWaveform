using System;
using System.Windows.Media;
using FontAwesome.Sharp;
using NWaveform.ViewModels;

namespace NWaveform.App
{
    public class WaypointLabelViewModel : LabelVievModel
    {
        public Waypoint Waypoint { get; }

        public WaypointLabelViewModel(Waypoint waypoint)
        {
            if (waypoint == null) throw new ArgumentNullException(nameof(waypoint));
            Waypoint = waypoint;

            Magnitude = 0.75;
            Tooltip = $"Waypoint: {Waypoint.Heading:F}°\r\n@Lon: {Waypoint.Reference.Latitude:F}, Lat: {Waypoint.Reference.Longitude:F}";
            Icon = IconChar.LocationArrow;
            Background = new SolidColorBrush(Colors.Green) {Opacity = 0.5};
            Foreground = new SolidColorBrush(Colors.DarkMagenta);
        }
    }

    public class Waypoint
    {
        public double Heading { get; }
        public Location Reference { get; }

        public Waypoint(double heading, Location reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            Heading = heading;
            Reference = reference;
        }
    }

    public class Location
    {
        public double Longitude { get; }
        public double Latitude { get; }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}