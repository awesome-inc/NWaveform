using System;
using System.Collections.Generic;
using FluentAssertions;
using NAudio.Wave;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.NAudio
{
    [TestFixtureFor(typeof(ResamplingExtensions))]
    // ReSharper disable once InconsistentNaming
    internal class ResamplingExtensions_Should
    {
        [Test]
        [TestCase(8000, 1, 8000)]
        [TestCase(8000, 2, 8000)]
        [TestCase(25000, 2, 4000)]
        [TestCase(8000, 2, 16000)]
        public void Resample(int inputSampleRate, int channels, int outputSampleRate)
        {
            var inputFormat = WaveFormat.CreateIeeeFloatWaveFormat(inputSampleRate, channels);
            var outputFormat = WaveFormat.CreateIeeeFloatWaveFormat(outputSampleRate, 1);

            var input = inputFormat.Generate(TimeSpan.FromSeconds(1));
            input.Should().HaveCount(inputSampleRate * channels * sizeof(float));

            var output = inputFormat.Resample(input, outputFormat);
            output.Should().HaveCount(outputSampleRate * sizeof(float));
        }

        [Test]
        public void Resample_linear()
        {
            var input = new List<float> { 1f, 0f, 1f, 0f };
            var output = input.Resampled(2, 1);
            var expected = new List<float> { 0.5f, 0.5f };
            output.Should().BeEquivalentTo(expected, "Linear downsample");

            input = new List<float> { 1f, 0f };
            output = input.Resampled(1, 2);
            expected = new List<float> { 1f, 1f, 0f, 0f };
            output.Should().BeEquivalentTo(expected, "linear upsample");
        }
    }
}
