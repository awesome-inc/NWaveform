using System;
using Autofac;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class StreamingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StreamingWaveProviderFactory>().As<IWaveProviderFactory>().SingleInstance();

            var channel1 = new EndlessFileLoopChannel("channel://1/", @"Data\Pulp_Fiction_Jimmys_Coffee.mp3", TimeSpan.FromMinutes(5));
            var channel2 = new EndlessFileLoopChannel("channel://2/", @"Data\Pulp_Fiction_Jimmys_Coffee.mp3", TimeSpan.FromMinutes(2));
            builder.RegisterInstance((IStreamingAudioChannel) channel1);
            builder.RegisterInstance((IStreamingAudioChannel) channel2);
        }
    }
}