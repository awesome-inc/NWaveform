using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Autofac;
using Autofac.Core;
using Caliburn.Micro;
using NWaveform.Default;
using NWaveform.Extender;
using NWaveform.Interfaces;
using NWaveform.NAudio;
using NWaveform.Serializer;
using NWaveform.ViewModels;
using NWaveform.Views;

namespace NWaveform.App
{
    public class AppBootstrapper : BootstrapperBase
    {
        private IContainer _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }

        protected override void Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<MainViewModel>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<WaveformPlayerViewModel>().As<IWaveformPlayerViewModel>();
            builder.RegisterType<PolygonWaveformViewModel>().As<IWaveformViewModel>();
            builder.RegisterType<AudioSelectionMenuProvider>().As<IAudioSelectionMenuProvider>().SingleInstance();

            AssemblySource.Instance.Add(typeof(WaveformPlayerView).Assembly);

            builder.RegisterType<WindowsMediaPlayer>().As<IMediaPlayer>().SingleInstance();
            //builder.RegisterType<NAudioPlayer>().As<IMediaPlayer>().SingleInstance();


            builder.RegisterType<CachedWaveFormRepository>().As<IWaveFormRepository>().SingleInstance();
            builder.RegisterType<NAudioWaveFormGenerator>().As<IWaveFormGenerator>().SingleInstance();
            builder.RegisterType<WaveFormSerializer>().As<IWaveFormSerializer>().SingleInstance();

            _container = builder.Build();
        }

        protected override object GetInstance(Type service, string key)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            if (string.IsNullOrWhiteSpace(key))
            {
                object result;
                if (_container.TryResolve(service, out result))
                    return result;
            }
            else
            {
                object result;
                if (_container.TryResolveNamed(key, service, out result))
                    return result;
            }
            throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, "Could not locate any instances of contract {0}.", key ?? service.Name));
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(new[] { service })) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }
    }
}