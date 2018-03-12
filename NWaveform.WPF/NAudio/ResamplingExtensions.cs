using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace NWaveform.NAudio
{
    public static class ResamplingExtensions
    {
        public static ISampleProvider GetSampleProvider(this WaveFormat waveFormat, byte[] data)
        {
            var waveProvider = new BufferedWaveProvider(waveFormat)
            {
                BufferLength = data.Length,
                ReadFully = false
            };
            waveProvider.ClearBuffer();
            waveProvider.AddSamples(data, 0, data.Length);
            var sampleProvider = waveProvider.ToSampleProvider();
            return sampleProvider;
        }

        public static byte[] Resample(this WaveFormat waveFormat, byte[] data, WaveFormat outputFormat)
        {
            if (waveFormat.Equals(outputFormat))
                return data;

            if (outputFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                throw new NotSupportedException("Only float supported.");
            if (outputFormat.Channels > 1)
                throw new NotSupportedException("Only mono supported.");

            var sampleProvider = waveFormat.GetSampleProvider(data);
            if (waveFormat.Channels > 1)
                sampleProvider = new StereoToMonoSampleProvider(sampleProvider);

            var numSamples = waveFormat.SampleRate;
            var buffer = new float[numSamples];

            var samples = new List<float>();
            var samplesRead = sampleProvider.Read(buffer, 0, buffer.Length);
            while (samplesRead > 0)
            {
                var floats = samplesRead == buffer.Length ? buffer : buffer.Take(samplesRead).ToArray();
                samples.AddRange(floats);
                samplesRead = sampleProvider.Read(buffer, 0, buffer.Length);
            }

            buffer = samples.Resampled(waveFormat.SampleRate, outputFormat.SampleRate);

            var output = new byte[buffer.Length * sizeof(float)];
            var waveBuffer = new WaveBuffer(output);
            for (var i = 0; i < buffer.Length; i++)
                waveBuffer.FloatBuffer[i] = buffer[i];
            return output;
        }

        internal static float[] Resampled(this IList<float> input, int inputSampleRate, int outputSampleRate)
        {
            if (inputSampleRate == outputSampleRate) return input.ToArray();
            var ratio = (double)outputSampleRate / inputSampleRate;
            var output = (ratio < 1.0) ? Downsample(input, ratio) : Upsample(input, ratio);
            return output;
        }

        private static float[] Downsample(IList<float> input, double ratio)
        {
            var output = new float[(int) (ratio * input.Count)];
            var j0 = 0;
            var sum = 0f;
            var n = 0;
            for (var i = 0; i < input.Count; i++)
            {
                var j = (int) (ratio * i);
                var y = input[i];
                if (j == j0)
                {
                    sum += y;
                    n++;
                }
                else
                {
                    output[j0] = sum / n;
                    sum = y;
                    j0 = j;
                    n = 1;
                }
            }
            if (j0 < output.Length) output[j0] = sum / n;
            return output;
        }

        private static float[] Upsample(IList<float> input, double ratio)
        {
            var output = new float[(int)(ratio * input.Count)];
            for (var j = 0; j < output.Length; j++)
            {
                var i = (int)(j / ratio);
                output[j] = input[i];
            }
            return output;
        }
    }
}
