using System;

namespace NWaveform.ViewModels
{
    public interface IAbsoluteTimeFormatter
    {
        string Format(DateTimeOffset? value);
    }
}