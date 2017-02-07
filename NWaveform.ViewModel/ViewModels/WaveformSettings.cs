using System.Windows.Media;

namespace NWaveform.ViewModels
{
    public class WaveformSettings
    {
        public double MaxMagnitude { get; set; }
        public double MaxError { get; set; }
        public double TicksEach { get; set; }

        public Color BackgroundColor { get; set; }
        public Color LeftColor { get; set; }
        public Color RightColor { get; set; }
        public Color PositionColor { get; set; }
        public Color SelectionColor { get; set; }
        public Color UserColor { get; set; }
        public Color SeparationLeftColor { get; set; }
        public Color SeparationRightColor { get; set; }
        public Color UserTextColor { get; set; }

        public WaveformSettings()
        {
            MaxMagnitude = 1.0;
            MaxError = 0.0;
            TicksEach = 5.0;

            BackgroundColor = Colors.DimGray;
            LeftColor = Colors.IndianRed;
            RightColor = Colors.RoyalBlue;
            PositionColor = Color.FromArgb(255, 192, 192, 0);// Colors.Yellow *0.8
            SelectionColor = Color.FromArgb(80, 115, 192, 115); // Colors.LimeGreen * 0.8
            UserColor = Color.FromArgb(192, 0, 0, 0);
            UserTextColor = Colors.White;

            SeparationLeftColor = Color.FromArgb(128, 0, 0, 0);
            SeparationRightColor = Color.FromArgb(128, 0, 0, 0);
        }
    }
}