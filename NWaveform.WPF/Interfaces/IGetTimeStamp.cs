using System;

namespace NWaveform.Interfaces
{
    public interface IGetTimeStamp
    {
        DateTimeOffset? For(Uri source);
    }
}