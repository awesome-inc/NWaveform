using System.Linq;
using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.NAudio
{
    [TestFixtureFor(typeof(SeekableCircularBuffer))]
    // ReSharper disable InconsistentNaming
    internal class SeekableCircularBuffer_Should
    {
        [Test]
        public void Clear_new_space_after_shift()
        {
            const int size = 8;
            var sut = new SeekableCircularBuffer(size);

            var expected = Enumerable.Range(0, sut.Length).Select(i => (byte) i).ToArray();

            sut.Write(expected, 0, expected.Length).Should().Be(sut.Length);

            var actual = new byte[sut.Length];
            sut.Read(actual, 0, actual.Length).Should().Be(sut.Length);
            actual.Should().Equal(expected);

            // now shift backward
            const int shift = size / 2;
            sut.Shift(shift);

            expected = expected.Skip(shift).Concat(Enumerable.Repeat((byte)0, size - shift)).ToArray();
            sut.ReadPosition = 0;
            sut.Read(actual, 0, actual.Length).Should().Be(sut.Length);
            actual.Should().Equal(expected);

            // shift forward
            sut.Shift(-shift);
            expected = Enumerable.Repeat((byte) 0, shift).Concat(expected.Take(size - shift)).ToArray();
            sut.ReadPosition = 0;
            sut.Read(actual, 0, actual.Length).Should().Be(sut.Length);
            actual.Should().Equal(expected);
        }
    }
}