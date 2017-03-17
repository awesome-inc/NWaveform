using System.Windows.Media;

namespace NWaveform.ViewModels
{
    public class WaveformSettings
    {
        public static readonly Color DefaultBackgroundColor = Colors.Transparent;
        public static readonly Color DefaultLeftColor = Colors.IndianRed;
        public static readonly Color DefaultRightColor = Colors.RoyalBlue;
        public static readonly Color DefaultTransparentBlack = Color.FromArgb(128, 0, 0, 0);

        public double TicksEach { get; set; } = 5.0;
        public double LiveDelta { get; set; } = 1.5;

        public Color BackgroundColor { get; set; } = DefaultBackgroundColor;
        public Color LeftColor { get; set; } = DefaultLeftColor;
        public Color RightColor { get; set; } = DefaultRightColor;
        public Color PositionColor { get; set; } = Color.FromArgb(255, 192, 192, 0);
        public Color SelectionColor { get; set; } = Color.FromArgb(80, 115, 192, 115);
        public Color UserColor { get; set; } = Color.FromArgb(192, 0, 0, 0);
        public Color SeparationLeftColor { get; set; } = DefaultTransparentBlack;
        public Color SeparationRightColor { get; set; } = DefaultTransparentBlack;
        public Color UserTextColor { get; set; } = Colors.White;
        public Color LastWriteColor { get; set; } = DefaultTransparentBlack;
    }
}