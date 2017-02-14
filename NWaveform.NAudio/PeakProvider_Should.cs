using System;
using FluentAssertions;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NEdifis;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.NAudio
{
    [TestFixtureFor(typeof(PeakProvider))]
    // ReSharper disable InconsistentNaming
    internal class PeakProvider_Should
    {
        [Test]
        [TestCase(44100, 2)]
        [TestCase(22050, 2)]
        [TestCase(16000, 2)]
        [TestCase(11025, 1)]
        [TestCase(8000, 1)]
        public void Provide_Average_Peaks_For(int rate, int channels)
        {
            var ctx = new ContextFor<PeakProvider>();
            var sut = ctx.BuildSut();

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(rate, channels);
            var sampleProvider = new SignalGenerator(rate, channels) { Frequency = 1, Type = SignalGeneratorType.Sin };
            var waveProvider = new SampleToWaveProvider(sampleProvider);
            var data = new byte[waveFormat.AverageBytesPerSecond];
            waveProvider.Read(data, 0, data.Length);

            sut.Filter = PeakProvider.AvgFilter;
            var peaks = sut.Sample(waveFormat, data);
            peaks.Should().HaveCount(sut.PeaksPerSecond);

            for (int i = 0; i < peaks.Length; i++)
            {
                // average of sin(x) over [a,b]:
                // = 1/(b-a) * \int_a^b sin(x) dx
                // = 1/(b-a) * [ -cos(b) - (-cos(a)) ]
                // = 1/(b-a) * [ cos(a) - cos(b) ]
                // = (cos(a) - cos(b)) / (b-a)

                var a = 2 * Math.PI * i / peaks.Length;
                var b = 2 * Math.PI * (i + 1) / peaks.Length;

                var expected = (float)((Math.Cos(a) - Math.Cos(b)) / (b - a));

                var peak = peaks[i];
                peak.Max.Should().BeInRange(-1f, 1f);
                peak.Min.Should().Be(-peak.Max);
                peak.Max.Should().BeApproximately(expected, 0.01f);
            }
        }

        [Test(Description = "Max magnitude (absolute value)")]
        public void Support_MagFilter()
        {
            PeakProvider.MagFilter(new[] { -1f }).Should().Be(1f);
            PeakProvider.MagFilter(new[] { -0.1f, 0.5f }).Should().Be(0.5f);
        }

        [Test]
        public void Support_Average_Filter()
        {
            PeakProvider.AvgFilter(new[] { -1f, 1f }).Should().Be(0f);
            PeakProvider.AvgFilter(new[] { 0.5f, 0.5f }).Should().Be(0.5f);
        }

        [Test]
        public void Support_Root_Mean_Square_Filter()
        {
            PeakProvider.RmsFilter(new[] { 0.5f, 0.5f }).Should().Be(0.5f);
            PeakProvider.RmsFilter(new[] { 0.25f, 0.5f, 0.75f, 1f }).Should().BeApproximately(0.684f, 0.001f);
        }

    }
}