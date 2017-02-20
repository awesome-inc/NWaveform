using FluentAssertions;
using NUnit.Framework;
using NWaveform.Model;

namespace NWaveform.Events
{
    [TestFixture()]
    // ReSharper disable InconsistentNaming
    internal class AudioSelectionEventArgs_Should
    {
        [Test()]
        public void AudioSelection_Should_Set_Parameter()
        {
            var sut = new AudioSelectionEventArgs(new AudioSelection(1, 3.0, 5.0));

            sut.AudioSelection.Should().NotBeNull();
            sut.AudioSelection.Channel.Should().Be(1);
            sut.AudioSelection.Start.Should().Be(3.0);
            sut.AudioSelection.End.Should().Be(5);
        }
    }
}