using System.IO;
using NWaveform.Model;

namespace NWaveform.Interfaces
{
    public interface IWaveFormSerializer
    {
        WaveformData Read(string source);
        void Save(Stream stream, WaveformData waveformData);
        string GetWaveFormUri(string strUri);
    }
}