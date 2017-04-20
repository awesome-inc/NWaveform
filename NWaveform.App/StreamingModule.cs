using System;
using System.Collections.Generic;
using System.Dynamic;
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
            builder.RegisterType<StreamingWaveProviderFactory>().AsImplementedInterfaces().SingleInstance();
            var v = new {TimeSpan = TimeSpan.FromSeconds(10), TimeShift = TimeSpan.FromHours(10)};
            var channels = new Dictionary<Uri, dynamic>
            {
                // we have channel 1 & 2 with a time shift because they are in the past (10 hours and 5 mins)
                { new Uri("channel://1/"), new { TimeSpan = TimeSpan.FromSeconds(10), TimeShift = TimeSpan.FromHours(-10)}},
                { new Uri("channel://2/"), new { TimeSpan = TimeSpan.FromSeconds(20), TimeShift = TimeSpan.FromMinutes(-5)}},
                { new Uri("channel://3/"), new { TimeSpan = TimeSpan.FromSeconds(33), TimeShift = TimeSpan.FromHours(0)} },
                { new Uri("channel://4/"), new { TimeSpan = TimeSpan.FromSeconds(1), TimeShift = TimeSpan.FromHours(0)} }
            };

            foreach (var kvp in channels)
            {
                builder.Register(c => CreateChannel(c, kvp.Key, kvp.Value.TimeSpan, kvp.Value.TimeShift)).As<IStreamingAudioChannel>();
                builder.Register(c => CreateChannelViewModel(c, kvp.Key, kvp.Value.TimeSpan.TotalSeconds)).As<IChannelViewModel>();
            }

            builder.RegisterType<SamplesHandlerPeakPublisher>().AsSelf().AutoActivate().SingleInstance();
            builder.RegisterType<PeakProvider>().As<IPeakProvider>();
        }

        private static EndlessFileLoopChannel CreateChannel(IComponentContext c, Uri source, TimeSpan bufferSize, TimeSpan timeShift)
        {
            var events = c.Resolve<IEventAggregator>();
            const string fileName = @"Data\Pulp_Fiction_Jimmys_Coffee.mp3";
            var preserveWrapAround = TimeSpan.FromSeconds(bufferSize.TotalSeconds * 0.9);
            return new EndlessFileLoopChannel(events, source, fileName, bufferSize, timeShift)
            { PreserveAfterWrapAround = preserveWrapAround };
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