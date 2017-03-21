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
        protected override void Load(ContainerBuilder builder)
        {
            var player = Environment.GetEnvironmentVariable("_PlayerType") ?? nameof(WaveOut);
            Type playerType = typeof(WaveOut);
            try
            {
                playerType = typeof(WaveOut).Assembly.GetType($"{typeof(WaveOut).Namespace}.{player}") ?? typeof(WaveOut);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Could not get player type '{player}': {ex}");
            }

            Trace.TraceInformation($"Using '{playerType}' as '{nameof(IWavePlayer)}'");
            builder.RegisterType(playerType).As<IWavePlayer>();

            builder.RegisterType<NAudioPlayer>().As<IMediaPlayer>();
            builder.RegisterType<NAudioGetWaveform>().As<IGetWaveform>().SingleInstance();
            builder.RegisterModule<StreamingModule>();

            builder.RegisterType<NAudioToMp3Cropper>().AsSelf().AutoActivate().SingleInstance();
        }
    }
}