using Autofac;
using NWaveform.Default;
using NWaveform.Interfaces;

namespace NWaveform.App
{
    internal class WindowsMediaPlayerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowsMediaPlayer>().As<IMediaPlayer>().SingleInstance();
        }
    }
}