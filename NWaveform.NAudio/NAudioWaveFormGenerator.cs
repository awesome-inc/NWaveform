using System;
using System.Collections.Generic;
using System.Globalization;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NWaveform.Extensions;
using NWaveform.Interfaces;
using NWaveform.Model;
using StreamVolumeEventArgs = NWaveform.Events.StreamVolumeEventArgs;

namespace NWaveform.NAudio
{
    public sealed class NAudioWaveFormGenerator : IWaveFormGenerator
    {
        public WaveformData Generate(string source, Action<StreamVolumeEventArgs> onProgress = null,
            int sampleRate = 20, int maxNumSamples = -1)
        {
            var fileName = new Uri(source).GetFileName(true);
            if (string.IsNullOrEmpty(fileName))
                throw new NotSupportedException("Remote Uris are not supported");

            using (var audioStream = new AudioFileReader(fileName))
            {
                return Generate(audioStream, source, onProgress, sampleRate, maxNumSamples);
            }
        }

        private static WaveformData Generate(WaveStream audioStream, string originalSource, Action<StreamVolumeEventArgs> onProgress, int sampleRate, int maxNumSamples)
        {
            var numChannels = audioStream.WaveFormat.Channels;

            if (numChannels != 1 && numChannels != 2)
                throw new ArgumentOutOfRangeException(nameof(audioStream), @"The audio stream should have one (=mono) or two (=stereo) channels.");

            var samples = new List<float>[numChannels];
            for (var channel = 0; channel < numChannels; channel++)
                samples[channel] = new List<float>();

            var rate = sampleRate < 0 ? (-audioStream.WaveFormat.SampleRate / sampleRate) : sampleRate;

            if (maxNumSamples > 0) // adjust sample rate so that a maximum of maxNumSamples is taken
            {
                var duration = audioStream.TotalTime.TotalSeconds;
                var expectedSamples = (long)(rate * duration);
                if (expectedSamples > maxNumSamples)
                {
                    rate = (int)(maxNumSamples / duration);
                }
            }

            var samplesPerNotification = (audioStream.WaveFormat.SampleRate / rate);

            var waveForm = CreateWaveformData(audioStream, originalSource, rate);

            var sampleStream = new SampleChannel(audioStream);

            // report 0%
            onProgress?.Invoke(new StreamVolumeEventArgs(0f, (float[])new float[numChannels].Clone()));

            var maxSamples = CreateSamples(audioStream, onProgress, numChannels, sampleStream, samplesPerNotification, samples);

            // report 100%
            onProgress?.Invoke(new StreamVolumeEventArgs(1f, (float[])maxSamples.Clone()));

            // copy channels to output
            waveForm.Channels = new Channel[numChannels];
            for (var channelIndex = 0; channelIndex < numChannels; channelIndex++)
                waveForm.Channels[channelIndex] = new Channel { Samples = samples[channelIndex].ToArray() };

            return waveForm;
        }

        private static float[] CreateSamples(
            WaveStream audioStream, Action<StreamVolumeEventArgs> onProgress, int numChannels,
            ISampleProvider sampleStream, int samplesPerNotification, IList<List<float>> samples)
        {
            var bufsize = numChannels * 8192;
            var buffer = new float[bufsize];
            int samplesRead;
            var sampleCount = 0;
            var maxSamples = new float[numChannels];

            do
            {
                var t0 = audioStream.CurrentTime.TotalSeconds; // save time before read
                samplesRead = sampleStream.Read(buffer, 0, bufsize);
                var t1 = audioStream.CurrentTime.TotalSeconds; // save time after read

                for (var channelIndex = 0; channelIndex + numChannels <= samplesRead; channelIndex += numChannels)
                {
                    // channel samples are interleaved, so just loop through and take the maximum
                    for (var sampleIndex = 0; sampleIndex < Math.Min(samplesRead, maxSamples.Length); sampleIndex++)
                    {
                        var sampleValue = buffer[channelIndex + sampleIndex];
                        maxSamples[sampleIndex] = Math.Max(maxSamples[sampleIndex], sampleValue);
                    }

                    sampleCount++;
                    if (sampleCount < samplesPerNotification) continue;

                    sampleCount = 0;

                    // calculate normalized position by lerping between t0 and t1 width t=i/samplesRead
                    var t = (double)channelIndex / samplesRead; // normalize w.r.t #samples
                    t = t0 + t * (t1 - t0); // lerp
                    t /= audioStream.TotalTime.TotalSeconds; // normalize by stream duration

                    for (var sampleIndex = 0; sampleIndex < maxSamples.Length; sampleIndex++)
                        samples[sampleIndex].Add(maxSamples[sampleIndex]);

                    // report current quantized volume
                    onProgress?.Invoke(new StreamVolumeEventArgs((float)t, (float[])maxSamples.Clone()));

                    Array.Clear(maxSamples, 0, maxSamples.Length); // reset max values to 0
                }
            } while (samplesRead > 0);
            return maxSamples;
        }

        private static WaveformData CreateWaveformData(WaveStream audioStream, string originalSource, int rate)
        {
            var waveForm = new WaveformData
            {
                Source = originalSource,
                Description = string.Format(CultureInfo.CurrentCulture, "Waveform generated from \"{0}\" at {1}", originalSource, DateTime.Now),
                Duration = audioStream.TotalTime,
                SampleRate = rate
            };
            return waveForm;
        }
    }
}
