namespace NWaveform.ViewModels
{
    public static class AudioSelectionViewModelExtensions
    {
        public static bool IsEmpty(this IAudioSelectionViewModel value)
        {
            return value.Start >= value.End;
        }
    }
}