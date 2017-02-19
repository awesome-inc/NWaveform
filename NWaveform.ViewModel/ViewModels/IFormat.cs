namespace NWaveform.ViewModels
{
    public interface IFormat<in T>
    {
        string Format(T value);
    }
}