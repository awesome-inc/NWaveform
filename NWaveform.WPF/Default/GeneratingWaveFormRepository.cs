using System;
using System.Diagnostics;
using NWaveform.Events;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.Default
{
    public class GeneratingWaveFormRepository : IWaveFormRepository
    {
        private readonly IWaveFormGenerator _generator;

        public GeneratingWaveFormRepository(IWaveFormGenerator generator = null)
        {
            // use fallback to NullObject implementation
            _generator = generator ?? new EmptyWaveFormGenerator();
        }

        public WaveformData For(Uri uri, Action<Progress> onProgress = null)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            var source = uri.ToString();

            if (_generator == null) return null; // don't want to throw here. Could be a live streaming?

            try
            {
                var waveformData = _generator.Generate(source, ToSample(onProgress));
                return waveformData;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Could not generate waveform: {0}", ex);
                return null;
            }
        }

        internal static Action<StreamVolumeEventArgs> ToSample(Action<Progress> onProgress)
        {
            Action<StreamVolumeEventArgs> onSample = null;
            if (onProgress != null) onSample = args => onProgress(new Progress((int)(100 * args.NormalizedPosition)));
            return onSample;
        }
    }
}