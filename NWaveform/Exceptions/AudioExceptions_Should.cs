using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using NWaveform.Events;

namespace NWaveform.Exceptions
{
    [TestFixture()]
    // ReSharper disable InconsistentNaming
    public class AudioExceptions_Should
    {
        [Test()]
        public void Be_The_Base_For_All_Audio_Exceptions()
        {
            var types = Assembly.GetAssembly(typeof(AudioEventArgs)).GetTypes()
                .Where(t => t != null && !t.IsAbstract && t.FullName.EndsWith("Exception")
                ).ToList();

            types.Count().Should().BeGreaterThan(1);
            types.ToList().ForEach(t =>
            {
                Console.WriteLine(@"testing type: {0}", t.FullName);

                var theBase = t;
                var found = false;
                while (theBase != typeof(object) && theBase != null)
                {
                    if (theBase == typeof(AudioException))
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