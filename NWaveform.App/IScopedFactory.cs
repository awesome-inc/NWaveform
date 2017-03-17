namespace NWaveform.App
{
    public interface IScopedFactory<T>
    {
        T Resolve();
        void Release(T item);
    }
}