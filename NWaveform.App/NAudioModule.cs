using Autofac;
using NWaveform.Interfaces;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class NAudioModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NAudioPlayer>().As<IMediaPlayer>();

            builder.RegisterModule<StreamingModule>();
        }
    }
}