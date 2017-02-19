using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Autofac;
using Autofac.Core;
using Caliburn.Micro;
using NWaveform.NAudio;
using NWaveform.Serializer;
using NWaveform.ViewModels;

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

            builder.Register(c => RegisterPlayer(c, "Player 1")).As<IPlayerViewModel>();
            builder.Register(c => RegisterPlayer(c, "Player 2")).As<IPlayerViewModel>();
            builder.Register(c => RegisterPlayer(c, "Player 3")).As<IPlayerViewModel>();
            builder.Register(c => RegisterPlayer(c, "Player 4")).As<IPlayerViewModel>();
            builder.Register(c => RegisterPlayer(c, "Player 5")).As<IPlayerViewModel>();

            builder.RegisterModule<AudioModule>();

            _container = builder.Build();
        }

        private static IPlayerViewModel RegisterPlayer(IComponentContext c, string name)
        {
            return new PlayerViewModel(c.Resolve<IWaveformPlayerViewModel>()) { DisplayName = name };
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