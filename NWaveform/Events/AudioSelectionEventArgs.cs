using NWaveform.Model;

namespace NWaveform.Events
{
    public class AudioSelectionEventArgs : AudioEventArgs
    {
        public AudioSelection AudioSelection { get; private set; }

        public AudioSelectionEventArgs(AudioSelection audioSelection)
        {
            AudioSelection = audioSelection;
        }
    }
}