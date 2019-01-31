using System;
using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.ViewModels
{
    [TestFixtureFor(typeof(DateTimeFormatter))]
    // ReSharper disable InconsistentNaming
    internal class DateTimeFormatter_Should
    {
        [Test]
        [TestCase("yyyy-MM-dd HH:mm:ss")]
        [TestCase("o")]
        public void Format(string formatString)
        {
            var sut = new DateTimeFormatter(formatString);
            var d = DateTimeOffset.UtcNow;
            sut.Format(d).Should().Be(d.ToString(formatString));
        }

        [Test]
        [TestCase(null)]
        [TestCase("\r\n\t")]
        [TestCase("z")]
        public void Throw_on_invalid(string formatString)
        {
            // cf.: https://msdn.microsoft.com/en-us/library/system.globalization.datetimeformatinfo(v=vs.110).aspx
            // ReSharper disable once ObjectCreationAsStatement
            0.Invoking(x => new DateTimeFormatter(formatString)).Should().Throw<FormatException>();
        }
    }
}
