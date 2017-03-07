using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class BufferedWaveStream : WaveStream
    {
        public override WaveFormat WaveFormat { get; }
        private readonly SeekableCircularBuffer _circularBuffer;
        private TimeSpan _preserveAfterWrapAround;

        public BufferedWaveStream(WaveFormat waveFormat, TimeSpan bufferDuration)
        {
            WaveFormat = waveFormat;
            BufferDuration = bufferDuration;
            _circularBuffer = new SeekableCircularBuffer(BufferLength);
        }

        #region Stream
        public override long Length => BufferLength;
        public override long Position {
            get { return _circularBuffer.ReadPosition; }
            set
            {
                value -= value % WaveFormat.BlockAlign;
                _circularBuffer.ReadPosition = (int) value;
            }
        }
        #endregion

        public event EventHandler WrappedAround;
        public TimeSpan PreserveAfterWrapAround
        {
            get { return _preserveAfterWrapAround; }
            set
            {
                if (value >= BufferDuration) throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(PreserveAfterWrapAround)} must be less than {nameof(BufferDuration)}");
                _preserveAfterWrapAround = value;
                PreservedBytes = (int)(value.TotalSeconds * WaveFormat.AverageBytesPerSecond);
                SkippedBytes = BufferLength - PreservedBytes;
                SkippedDuration = BufferDuration - _preserveAfterWrapAround;
            }
        }

        public int PreservedBytes { get; private set; }
        public TimeSpan SkippedDuration { get; private set; }
        public int SkippedBytes { get; private set; }

        protected virtual void OnWrappedAround()
        {
            WrappedAround?.Invoke(this, EventArgs.Empty);
        }

        public long WritePosition => _circularBuffer.WritePosition;
        public TimeSpan CurrentWriteTime => TimeSpan.FromSeconds((double)WritePosition / WaveFormat.AverageBytesPerSecond);

        #region Reimplementation of BufferedWaveProvider

        public int BufferLength { get; private set; }
        public TimeSpan BufferDuration
        {
            get { return TimeSpan.FromSeconds((double)BufferLength / WaveFormat.AverageBytesPerSecond); }
            private set { BufferLength = (int)(value.TotalSeconds * WaveFormat.AverageBytesPerSecond); }
        }


        public int AddSamples(byte[] buffer, int offset = 0, int length = 0)
        {
            var count = length > 0 ? length : buffer.Length;
            
            // definition by cases
            var exceeding = (int)(WritePosition + count - BufferLength);
            if (exceeding < 0 || PreservedBytes == 0)
                return _circularBuffer.Write(buffer, offset, count);
            
            // split in two writes: a) until full, b) exceeding
            var delta = count-exceeding;
            var written = _circularBuffer.Write(buffer, offset, delta);
            _circularBuffer.Shift(-SkippedBytes);

            OnWrappedAround();

            if (exceeding > 0)
                written += _circularBuffer.Write(buffer, offset + delta, exceeding);

            return written;
        }

        public override int Read(byte[] buffer, int offset, int length)
        {
            var count = length > 0 ? length : buffer.Length;
            var read = _circularBuffer.Read(buffer, offset, count);
            if (read < count)
            {
                // zero the end of the buffer
                Array.Clear(buffer, offset + read, count - read);
                read = count;
            }
            return read;
        }

        public void ClearBuffer()
        {
            _circularBuffer?.Clear();
        }
        #endregion
    }
}