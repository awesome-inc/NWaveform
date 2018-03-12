namespace NWaveform.Model
{
    public class Progress
    {
        public Progress(int progressPercentage)
        {
            ProgressPercentage = progressPercentage;
        }

        public int ProgressPercentage { get; }
    }
}
