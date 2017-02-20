using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace NWaveform.Events
{
    [TestFixture()]
    // ReSharper disable InconsistentNaming
    public class AudioEventArgs_Should
    {
        [Test()]
        public void Be_The_Base_For_All_Audio_Events()
        {
            var types = Assembly.GetAssembly(typeof(AudioEventArgs)).GetTypes()
                .Where(t => t != null && !t.IsAbstract && t.FullName.EndsWith("EventArgs")
                ).ToList();

            types.Count().Should().BeGreaterThan(1);
            types.ToList().ForEach(t =>
            {
                var theBase = t;
                var found = false;
                while (theBase != typeof(object) && theBase != null)
                {
                    if (theBase == typeof(AudioEventArgs))
                    {
                        found = true;
                        break;
                    }

                    theBase = theBase.BaseType;
                }

                found.Should().BeTrue();
            });
        }
    }
}