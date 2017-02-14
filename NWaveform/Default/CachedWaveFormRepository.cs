using System;
using System.Diagnostics;
using System.IO;
using NWaveform.Events;
using NWaveform.Extensions;
using NWaveform.Interfaces;
using NWaveform.Model;
using NWaveform.Serializer;

namespace NWaveform.Default
{
    /// <summary>
    /// A repository for handling a waveform in an optimized way like caching. Due to caching,
    /// the repository is immutable with respect to the generator.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CachedWaveFormRepository : IWaveFormRepository
    {
        private readonly IWaveFormSerializer _serializer;
        // question: how does the cache work with different audio files?
        private readonly IWaveFormGenerator _generator;

        internal Func<string, Stream> StreamFor { get; set; }

        public CachedWaveFormRepository(IWaveFormGenerator generator = null, IWaveFormSerializer serializer = null)
        {
            // use fallback to NullObject implementation
            _generator = generator ?? new EmptyWaveFormGenerator();

            // use fallback to binary serializer
            _serializer = serializer ?? new WaveFormSerializer();

            StreamFor = GetStreamFor;
        }

        public WaveformData For(Uri uri, Action<Progress> onProgress = null)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            var source = uri.ToString();
            string waveFormUri = null;

            try
            {
                waveFormUri = _serializer.GetWaveFormUri(source);
                return _serializer.Read(waveFormUri);
            }
            catch (Exception readException)
            {
                var msg = readException is FileNotFoundException ? readException.Message : readException.ToString();
                Trace.TraceInformation(
                    "Could not read cached waveform \"{1}\" for media \"{0}\": {2}",
                    source, waveFormUri, msg);
            }

            if (_generator == null) return null; // don't want to throw here. Could be a live streaming?

            try
            {
                var waveformData = _generator.Generate(source, ToSample(onProgress));
                Cache(waveformData, waveFormUri);
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

        private void Cache(WaveformData waveformData, string destination)
        {
            if (WaveformData.IsNullOrEmpty(waveformData) || string.IsNullOrEmpty(destination)) return;

            try
            {
                using (var stream = StreamFor(destination)) _serializer.Save(stream, waveformData);
                Trace.WriteLine("Waveform cached to {0}".FormatWith(destination));

            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Could not cache waveform to \"{0}\": {1}", destination, ex);
            }
        }

        private static Stream GetStreamFor(string destination)
        {
            var fileName = new Uri(destination).GetFileName(false);
            return string.IsNullOrEmpty(fileName) 
                ? null
                : new FileStream(fileName, FileMode.Create);
        }
    }
}