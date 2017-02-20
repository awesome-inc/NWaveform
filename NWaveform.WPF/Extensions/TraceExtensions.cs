using System.Globalization;

namespace NWaveform.Extensions
{
    internal static class TraceExtensions
    {
        public static string FormatWith(this string source, params object[] paramObjects)
        {
            return string.Format(CultureInfo.CurrentCulture, source, paramObjects);
        }
    }
}