using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class WavePlayerFactory<TPlayer> : IWavePlayerFactory where TPlayer : IWavePlayer, new()
    {
        public IWavePlayer Create()
        {
            return new TPlayer();
        }
    }

    public class WavePlayerFactory : IWavePlayerFactory
    {
        // cf.
        // - https://github.com/naudio/NAudio/wiki/Understanding-Output-Devices
        // - http://markheath.net/post/naudio-audio-output-devices
        public static readonly Type DefaultPlayerType = typeof(WaveOut);

        private readonly Type _playerType;

        public WavePlayerFactory(Type playerType = null)
        {
            var safePlayerType = playerType ?? DefaultPlayerType;
            if (!typeof(IWavePlayer).IsAssignableFrom(safePlayerType)) throw new ArgumentException($"The specified type '{safePlayerType}' must implement '{nameof(IWavePlayer)}'.");
            _playerType = safePlayerType;
        }

        public IWavePlayer Create()
        {
            return (IWavePlayer) Activator.CreateInstance(_playerType);
        }
    }
}