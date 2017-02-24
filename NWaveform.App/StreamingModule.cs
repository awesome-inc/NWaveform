using System;
using System.Collections.Generic;
using Autofac;
using Caliburn.Micro;
using NWaveform.NAudio;
using NWaveform.ViewModels;

namespace NWaveform.App
{
    internal class StreamingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StreamingWaveProviderFactory>().As<IWaveProviderFactory>().SingleInstance();

            var channels = new Dictionary<Uri, TimeSpan>
            {
                { new Uri("channel://1/"), TimeSpan.FromSeconds(10)},
                { new Uri("channel://2/"), TimeSpan.FromSeconds(20)},
                { new Uri("channel://3/"), TimeSpan.FromSeconds(33)},
                { new Uri("channel://4/"), TimeSpan.FromMinutes(1)}
            };

            foreach (var kvp in channels)
            {
                builder.Register(c => CreateChannel(c, kvp.Key, kvp.Value)).As<IStreamingAudioChannel>();
                builder.Register(c => CreateChannelViewModel(c, kvp.Key, kvp.Value.TotalSeconds)).As<IChannelViewModel>();
            }

            builder.RegisterType<SamplesHandlerPeakPublisher>().AsSelf().AutoActivate().SingleInstance();
            builder.RegisterType<PeakProvider>().As<IPeakProvider>();
        }

        private static EndlessFileLoopChannel CreateChannel(IComponentContext c, Uri source, TimeSpan bufferSize)
        {
            var events = c.Resolve<IEventAggregator>();
            const string fileName = @"Data\Pulp_Fiction_Jimmys_Coffee.mp3";
            //var preserveWrapAround = TimeSpan.FromSeconds(bufferSize.TotalSeconds / 3.0);
            return new EndlessFileLoopChannel(events, source, fileName, bufferSize);
            //{ PreserveAfterWrapAround = preserveWrapAround };
        }

        private static IChannelViewModel CreateChannelViewModel(IComponentContext c, Uri source, double duration)
        {
            var waveform = c.Resolve<IWaveformDisplayViewModel>();
            waveform.Source = source;
            waveform.Duration = duration;
            return new ChannelViewModel(waveform);
        }
    }
}