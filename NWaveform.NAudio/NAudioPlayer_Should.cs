using System;
using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;
using NWaveform.Exceptions;

namespace NWaveform.NAudio
{
    [TestFixtureFor(typeof(NAudioPlayer))]
    // ReSharper disable InconsistentNaming
    internal class NAudioPlayer_Should
    {
        [Test]
        public void Wrap_media_exceptions_into_AudioException()
        {
            var sut = new NAudioPlayer();
            sut.Error.HasException.Should().BeFalse();

            sut.Source = new Uri(@"c:\does.not.exist.wav");

            sut.Error.HasException.Should().BeTrue();
            sut.Error.Exception.Should().BeOfType<AudioException>();
            sut.Error.Exception.Message.Should().StartWith("Could not open audio");

            sut.Source = null;
            sut.Error.HasException.Should().BeFalse();
        }
    }
}
