#if VLC
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Autofac;
using NWaveform.Interfaces;
using NWaveform.Vlc;

namespace NWaveform.App
{
    internal class VlcModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<VlcMediaPlayer>().As<IMediaPlayer>();

            new VlcConfiguration().VerifyVlcPresent();

            try
            {
                // register AudioEndPointVolume (requires logged on user)
                if (Environment.UserInteractive)
                    builder.RegisterInstance(CoreAudio.GetDefaultVolumeEndpoint());
            }
            catch (COMException ex)
            {
                Trace.TraceError($"Well, do you have a sound card installed? Make sure the service is running (esp. when you running the app on a server): {ex}");
            }
        }
    }
}
#endif