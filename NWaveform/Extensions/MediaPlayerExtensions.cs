using System;
using NWaveform.Interfaces;

namespace NWaveform.Extensions
{
    public static class MediaPlayerExtensions
    {
        public static void TogglePlayPause(this IMediaPlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (player.IsPlaying)
            {
                player.Pause();
            }
            else
            {
                player.Play();
            }
        }

        public static void ToggleMute(this IMediaPlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (player.IsMuted)
            {
                player.UnMute();
            }
            else
            {
                player.Mute();
            }
        }

    }
}