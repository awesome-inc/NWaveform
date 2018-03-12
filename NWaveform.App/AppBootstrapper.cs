using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using Autofac;
using Autofac.Core;
using Caliburn.Micro;
using NLog;
using NLog.Config;
using NLog.Targets;
using NWaveform.App.MiniMods;

namespace NWaveform.App
{
    public class AppBootstrapper : BootstrapperBase
    {
        private IContainer _container;

        public AppBootstrapper()
        {
            InitializeLogging();

            Initialize();
        }

        private static void InitializeLogging()
        {
            var caliburnLogger = new TraceLogger();
            Caliburn.Micro.LogManager.GetLog = type => caliburnLogger;
            NLog.LogManager.Configuration = DefaultLogging();
            Trace.Listeners.Add(new NLogTraceListener());
        }

        private static LoggingConfiguration DefaultLogging(LogLevel logLevel = null)
        {
            // cf.: http://stackoverflow.com/questions/24070349/nlog-switching-from-nlog-config-to-programmatic-configuration
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget
            {
                Layout = "${longdate} [${threadid}] ${uppercase:${level}} - ${message}",
                FileName = "NWaveform.App.log",
                Header = "[Open Log]",
                Footer = "[Close Log]",
                ArchiveFileName = "NWaveform.App.{#}.log",
                ArchiveAboveSize = 1048576,
                ArchiveEvery = FileArchivePeriod.None,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                MaxArchiveFiles = 5,
                ConcurrentWrites = false,
                KeepFileOpen = true,
                Encoding = Encoding.UTF8
            };

            config.AddTarget("f", fileTarget);

            var rule = new LoggingRule("*", logLevel ?? LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule);

            return config;
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
            builder.RegisterGeneric(typeof(ScopedFactory<>)).As(typeof(IScopedFactory<>)).SingleInstance();
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
                if (_container.TryResolve(service, out var result))
                    return result;
            }
            else
            {
                if (_container.TryResolveNamed(key, service, out var result))
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
