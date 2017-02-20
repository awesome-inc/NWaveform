using System;
using System.IO;
using FluentAssertions;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;
using NWaveform.Events;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.Default
{
    [TestFixtureFor(typeof(CachedWaveFormRepository))]
    // ReSharper disable InconsistentNaming
    internal class CachedWaveFormRepository_Should
    {
        [Test]
        public void First_try_to_get_cached_waveform_via_serializer()
        {
            var uri = new Uri("http://test/uri");
            var expected = new WaveformData();
            var context = new CreationContext();
            context.Serializer.GetWaveFormUri(Arg.Any<string>()).Returns(x => x.Arg<string>());
            context.Serializer.Read(uri.ToString()).Returns(expected);

            var sut = context.Create();

            var actual = sut.For(uri);
            actual.Should().BeSameAs(expected);
        }

        [Test]
        public void Try_generate_and_cache_waveform_on_CacheMiss()
        {
            var uri = new Uri("http://test/uri");
            var expected = new WaveformData
            {
                Duration = TimeSpan.FromSeconds(1),
                Channels = new[] { new Channel() }
            };
            WaveformData.IsNullOrEmpty(expected).Should().BeFalse();

            var context = new CreationContext();
            context.Serializer.GetWaveFormUri(Arg.Any<string>()).Returns(x => x.Arg<string>());
            context.Serializer.Read(uri.ToString()).Returns(x => { throw new InvalidOperationException("test"); });
            var progress = 0;
            Action<Progress> onProgress = p => progress = p.ProgressPercentage;

            context.Generator.Generate(uri.ToString(), Arg.Any<Action<StreamVolumeEventArgs>>()).Returns(x =>
                {
                    var onSample = x.Arg<Action<StreamVolumeEventArgs>>();
                    for (int i = 0; i <= 10; i++)
                        onSample(new StreamVolumeEventArgs(i * 0.1f, null));
                    return expected;
                });

            var stream = Substitute.For<Stream>();
            var sut = context.Create();
            sut.StreamFor = s => s == uri.ToString() ? stream : null;

            var actual = sut.For(uri, onProgress);
            // check a waveform has been cached
            context.Serializer.Received().Save(stream, expected);
            progress.Should().Be(100);
            actual.Should().BeSameAs(expected);
        }

        [Test]
        public void Return_null_if_both_CacheMiss_and_generation_fails()
        {
            var uri = new Uri("http://test/uri");
            var context = new CreationContext();
            context.Serializer.GetWaveFormUri(Arg.Any<string>()).Returns(x => x.Arg<string>());
            context.Serializer.Read(uri.ToString()).Returns(x => { throw new InvalidOperationException("test read"); });
            context.Generator.Generate(uri.ToString()).Returns(x => { throw new InvalidOperationException("test generate"); });

            var sut = context.Create();

            sut.For(uri).Should().BeNull();
        }


        private class CreationContext
        {
            public IWaveFormGenerator Generator { get; set; }
            public IWaveFormSerializer Serializer { get; set; }

            public CreationContext()
            {
                Generator = Substitute.For<IWaveFormGenerator>();
                Serializer = Substitute.For<IWaveFormSerializer>();

            }
            public CachedWaveFormRepository Create()
            {
                return new CachedWaveFormRepository(Generator, Serializer);
            }
        }
    }
}