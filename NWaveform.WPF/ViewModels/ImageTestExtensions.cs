using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NUnit.Framework;

namespace NWaveform.ViewModels
{
    internal static class ImageTestExtensions
    {
        public static unsafe void RectShouldHaveColor(this WriteableBitmap b, int x0, int y0, int x1, int y1, Color color)
        {
            var expectedColor = WriteableBitmapExtensions.ConvertColor(color);
            var expectedColorName = GetColorName(color);
            using (var c = b.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                x1 = Clamp(x1, 0, c.Width - 1);
                y1 = Clamp(y1, 0, c.Height - 1);
                x0 = Clamp(x0, 0, x1);
                y0 = Clamp(y0, 0, y1);

                for (var y = y0; y < y1; y++)
                    for (var x = x0; x < x1; x++)
                    {
                        var actualColor = c.Pixels[y * c.Width + x];
                        if (actualColor != expectedColor)
                        {
                            var actualColorName = GetColorName(b.GetPixel(x, y));
                            Assert.Fail($"Pixel at ({x},{y}) should be '{expectedColorName}' but is '{actualColorName}'");
                        }
                    }
            }
        }

        public static void ShouldHaveColor(this WriteableBitmap b, Color color)
        {
            RectShouldHaveColor(b, 0, 0, b.PixelWidth, b.PixelHeight, color);
        }

        static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static string GetColorName(Color col)
        {
            var colorProperty = typeof(Colors).GetProperties()
                .FirstOrDefault(p => Color.AreClose((Color)p.GetValue(null), col));
            return colorProperty != null ? colorProperty.Name : col.ToString();
        }
    }
}