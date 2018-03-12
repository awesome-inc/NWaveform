using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class WaveProviderEx : WaveChannel32, IWaveProviderEx
    {
        public WaveProviderEx(Uri source)
            : this(new AudioStream(source))
        {
        }

        public WaveProviderEx(WaveStream baseStream) : base(baseStream)
        {}

        public bool SupportsPanning { get; } = true;

        public void ExplicitClose()
        {
            base.Close();
        }

        #region override Stream's IDisposable pattern
        public override void Close()
        {
            if (!Closeable) return;
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (!Closeable) return;
            base.Dispose(disposing);
        }

        public bool Closeable { get; set; } = true;

        #endregion
    }
}
