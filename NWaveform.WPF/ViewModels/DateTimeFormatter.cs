using System;

namespace NWaveform.ViewModels
{
    public class DateTimeFormatter : IAbsoluteTimeFormatter
    {
        public DateTimeFormatter(string formatString, string noValueString = null)
        {
            ValidateFormatString(formatString);
            FormatString = formatString;
            NoValueString = noValueString ?? string.Empty;
        }

        public string Format(DateTimeOffset? value)
        {
            return value?.ToString(FormatString) ?? NoValueString;
        }

        public string FormatString { get; }
        public string NoValueString { get; }

        private static void ValidateFormatString(string formatString)
        {
            if (string.IsNullOrWhiteSpace(formatString))
                throw new FormatException("Datetime format must not be null, empty or whitespace");
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            DateTimeOffset.UtcNow.ToString(formatString);
        }

    }
}