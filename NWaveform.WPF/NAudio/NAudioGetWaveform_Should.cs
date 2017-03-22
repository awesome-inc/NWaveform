using System;
using System.Linq;
using FluentAssertions;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NEdifis;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;

namespace NWaveform.NAudio
{
    [TestFixtureFor(typeof(NAudioGetWaveform))]
    // ReSharper disable InconsistentNaming
    internal class NAudioGetWaveform_Should
    {
        [Test]
        public void Use_full_volume_and_center_balance_on_sampling()
        {
            var ctx = new ContextFor<NAudioGetWaveform>();
            var peakProvider = new PeakProvider();
            ctx.Use<IPeakProvider>(peakProvider);
            var sut = ctx.BuildSut();

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8000, 1);

            using (var buffer = new BufferedWaveStream(waveFormat, TimeSpan.FromSeconds(1)))
            using (var waveStream = new WaveProviderEx(buffer))
            {
                var source = new Uri("my://source/");
                ctx.For<IWaveProviderFactory>().Create(source).Returns(waveStream);

                waveStream.Position = 40;
                waveStream.Volume = 0.5f;
                waveStream.Pan = -1f;

                var samples = waveFormat.Generate(buffer.BufferDuration, SignalGeneratorType.Square);
                buffer.AddSamples(samples, 0, samples.Length);

                var waveForm = sut.For(source);
                waveForm.Duration.Should().Be(buffer.BufferDuration);
                waveForm.Peaks.Should().HaveCount((int)(peakProvider.PeaksPerSecond * buffer.BufferDuration.TotalSeconds));
                waveForm.Peaks.All(p => p.Max == 1f && p.Min == -1f).Should().BeTrue();

                waveStream.Position.Should().Be(40, "should be restored after sampling");
                waveStream.Volume.Should().Be(0.5f);
                waveStream.Pan.Should().Be(-1f);
            }
        }

    }
}