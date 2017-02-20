using Autofac;
using NAudio.Wave;
using NWaveform.Interfaces;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class NAudioModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterType<DirectSoundOut>().As<IWavePlayer>();
            builder.RegisterType<WaveOut>().As<IWavePlayer>();

            builder.RegisterType<NAudioPlayer>().As<IMediaPlayer>();
            builder.RegisterType<NAudioWaveFormGenerator>().As<IWaveFormGenerator>().SingleInstance();
            builder.RegisterModule<StreamingModule>();

            builder.RegisterType<NAudioToMp3Cropper>().AsSelf().AutoActivate().SingleInstance();
        }
    }
}