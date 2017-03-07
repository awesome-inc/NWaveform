using System;
using System.IO;
using NWaveform.Extensions;
using NWaveform.Interfaces;

namespace NWaveform.NAudio
{
    public class WaveProviderFactory : IWaveProviderFactory, IGetTimeStamp
    {
        public virtual IWaveProviderEx Create(Uri source)
        {
            return new WaveProviderEx(source);
        }

        public virtual DateTimeOffset? For(Uri source)
        {
            if (source == null) return null;
            var fileName = source.GetFileName(true);
            if (string.IsNullOrEmpty(fileName)) return null;
            try { return File.GetCreationTimeUtc(fileName); }
            catch { return null; }
        }
    }
}