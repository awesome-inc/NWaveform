using System;
using System.Globalization;

namespace NWaveform.Model
{
    public struct AudioSelection : IEquatable<AudioSelection>
    {
        public static readonly AudioSelection Empty = new AudioSelection(-1, 0, 0);
        public static double SnapTolerance = 0.25; // 0.1, 0.25

        public int Channel; // -1: all channels
        public double Start;
        public double End;

        public bool IsEmpty => (End <= Start);

        public AudioSelection(int channel, double start, double end)
        {
            Channel = channel;
            Start = start;
            End = end;
        }

        public bool Contains(double position)
        {
            return (position >= Start && position <= End);
        }

        public double SnapTo(double position)
        {
            if (position + SnapTolerance < Start)
                return Start;
            return position > End + SnapTolerance ? End : position;
        }

        public double Duration => End - Start;

        public bool Equals(AudioSelection other)
        {
            return (Math.Abs(Start - other.Start) < double.Epsilon &&
                Math.Abs(End - other.End) < double.Epsilon);
        }

        public override bool Equals(object obj)
        {
            return obj is AudioSelection && Equals((AudioSelection)obj);
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode();
        }

        public static bool operator ==(AudioSelection firstSelection, AudioSelection secondSelection)
        {
            return firstSelection.Equals(secondSelection);
        }

        public static bool operator !=(AudioSelection firstSelection, AudioSelection secondSelection)
        {
            return !firstSelection.Equals(secondSelection);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{Start: {0}, End: {1}, Channel: {2}}}",
                FormatString(Start), FormatString(End), Channel);
        }

        private static string FormatString(double value)
        {
            var ts = TimeSpan.FromSeconds(value);
            return string.Format(CultureInfo.CurrentCulture, "{0:00}:{1:00}:{2:00}.{3:0}",
                ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 100);
        }
    }
}
