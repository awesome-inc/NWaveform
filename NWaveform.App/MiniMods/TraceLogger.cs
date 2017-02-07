using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;

namespace NWaveform.App.MiniMods
{
    // ReSharper disable once UnusedMember.Global
    [ExcludeFromCodeCoverage]
    public class TraceLogger : ILog
    {
        public void Error(Exception exception)
        {
            Trace.TraceError(exception.ToString());
        }

        public void Info(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Trace.TraceWarning(format, args);
        }
    }
}
