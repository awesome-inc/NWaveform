using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Autofac;
using Autofac.Core;
using Caliburn.Micro;

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
            DisplayRootViewFor<AppViewModel>();
        }

        protected override void Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            builder.RegisterType<AppViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<PlayerViewModel>().As<IPlayerViewModel>();
            builder.RegisterType<ChannelsViewModel>().As<IChannelsViewModel>();

            builder.RegisterModule<AudioModule>();

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
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }
    }
}