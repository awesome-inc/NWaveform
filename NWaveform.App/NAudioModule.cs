using System;
using System.Diagnostics;
using Autofac;
using NAudio.Wave;
using NWaveform.Interfaces;
using NWaveform.NAudio;

namespace NWaveform.App
{
    internal class NAudioModule : Module
    {
        private static readonly Type DefaultPlayerType = WavePlayerFactory.DefaultPlayerType;

        protected override void Load(ContainerBuilder builder)
        {
            var playerType = DefaultPlayerType;
            var player = Environment.GetEnvironmentVariable("_PlayerType") ?? playerType.Name;
            try
            {
                playerType = typeof(WaveOut).Assembly.GetType($"{DefaultPlayerType.Namespace}.{player}") ?? DefaultPlayerType;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Could not get player type '{player}': {ex}");
            }

            Trace.TraceInformation($"Using '{playerType}' as '{nameof(IWavePlayer)}'");
            builder.Register(c => new WavePlayerFactory(playerType)).As<IWavePlayerFactory>();

            builder.RegisterType<NAudioPlayer>().As<IMediaPlayer>();
            builder.RegisterType<NAudioGetWaveform>().As<IGetWaveform>().SingleInstance();
            builder.RegisterModule<StreamingModule>();

            builder.RegisterType<NAudioToMp3Cropper>().AsSelf().AutoActivate().SingleInstance();
        }
    }
}