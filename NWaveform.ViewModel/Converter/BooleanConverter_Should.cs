using System.Globalization;
using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.Converter
{
    [TestFixtureFor(typeof (BooleanConverter<>))]
    // ReSharper disable once InconsistentNaming
    internal class BooleanConverter_Should
    {
        [Test]
        public void Convert_True_To_Visible()
        {
            var sut = new BooleanConverter<string>("yes", "no");

            var result = sut.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);

            result.Should().BeOfType<string>();
            result.Should().Be("yes");
        }

        [Test]
        public void Convert_False_To_Collapsed()
        {
            var sut = new BooleanConverter<string>("yes", "no");

            var result = sut.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);

            result.Should().BeOfType<string>();
            result.Should().Be("no");
        }
    }
}