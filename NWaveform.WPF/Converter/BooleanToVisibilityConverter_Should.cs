using System.Globalization;
using System.Windows;
using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.Converter
{
    [TestFixtureFor(typeof(BooleanToVisibilityConverter))]
    // ReSharper disable once InconsistentNaming
    internal class BooleanToVisibilityConverter_Should
    {
        [Test]
        public void Convert_True_To_Visible()
        {
            var sut = new BooleanToVisibilityConverter();

            var result = sut.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture);

            result.Should().BeOfType<Visibility>();
            result.Should().Be(Visibility.Visible);
        }

        [Test]
        public void Convert_False_To_Collapsed()
        {
            var sut = new BooleanToVisibilityConverter();

            var result = sut.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture);

            result.Should().BeOfType<Visibility>();
            result.Should().Be(Visibility.Collapsed);
        }

    }
}