using Autofac;
using Caliburn.Micro;
using NWaveform.Extender;
using NWaveform.ViewModels;
using NWaveform.Views;

namespace NWaveform.App
{
    public class AudioModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WaveformPlayerViewModel>().As<IWaveformPlayerViewModel>();
            builder.RegisterType<WaveformViewModel>().As<IWaveformViewModel>();
            builder.RegisterType<WaveformDisplayViewModel>().As<IWaveformDisplayViewModel>();
            builder.RegisterType<AudioSelectionMenuProvider>().As<IAudioSelectionMenuProvider>().SingleInstance();

            builder.Register(c => new DateTimeFormatter("yyyy-MM-dd HH:mm:ss"))
                .As<IAbsoluteTimeFormatter>()
                .SingleInstance();

            AssemblySource.Instance.Add(typeof(WaveformPlayerView).Assembly);

            //builder.RegisterModule<WindowsMediaPlayerModule>();
            builder.RegisterModule<NAudioModule>();
            //builder.RegisterModule<VlcModule>();
        }
    }
}