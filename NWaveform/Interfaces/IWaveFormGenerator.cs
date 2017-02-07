using System;
using NWaveform.Events;
using NWaveform.Model;

namespace NWaveform.Interfaces
{
    public interface IWaveFormGenerator
    {
        /// <summary>
        ///     Generates a <see cref="WaveformData" /> from the specified source.
        /// </summary>
        /// <param name="source">The source, e.g. filename or uri.</param>
        /// <param name="onProgress">progress callback</param>
        /// <param name="sampleRate">The sample rate</param>
        /// <param name="maxNumSamples">
        ///     The maximum number of samples to take (-1 to disable).
        ///     Use this to to bound the total number of generated samples, e.g. for very long audios.
        ///     When used for displaying the waveform on screen a good value should be around the maximum
        ///     horizontal screen resolution in pixels (e.g. 1920 for Full HD).
        /// </param>
        WaveformData Generate(string source, Action<StreamVolumeEventArgs> onProgress = null,
            int sampleRate = 20, int maxNumSamples = -1);
    }
}