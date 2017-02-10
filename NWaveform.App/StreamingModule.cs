using System;
using Autofac;
using Caliburn.Micro;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class StreamingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StreamingWaveProviderFactory>().As<IWaveProviderFactory>().SingleInstance();

            builder.Register(c => RegisterChannel(c, "channel://1/", TimeSpan.FromMinutes(2))).As<IStreamingAudioChannel>();
            builder.Register(c => RegisterChannel(c, "channel://2/", TimeSpan.FromMinutes(5))).As<IStreamingAudioChannel>();

            builder.RegisterType<SamplesHandlerPeakPublisher>().AsSelf().AutoActivate();
            builder.RegisterType<PeakProvider>().As<IPeakProvider>();
        }

        private static EndlessFileLoopChannel RegisterChannel(IComponentContext c, string name, TimeSpan bufferSize)
        {
            var events = c.Resolve<IEventAggregator>();
            const string fileName = @"Data\Pulp_Fiction_Jimmys_Coffee.mp3";
            return new EndlessFileLoopChannel(events, name, fileName, bufferSize);
        }
    }
}