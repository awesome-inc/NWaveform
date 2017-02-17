using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class BufferedWaveStream : WaveStream
    {
        public override WaveFormat WaveFormat { get; }
        private readonly SeekableCircularBuffer _circularBuffer;
        private TimeSpan _preserveAfterWrapAround;
        private byte[] _preservedBuffer;

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
                value = Math.Min(value, Length);
                value -= value % WaveFormat.BlockAlign;
                value = Math.Max(0, value);
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
                var preservedBytes = (int)(value.TotalSeconds * WaveFormat.AverageBytesPerSecond);
                _preservedBuffer = preservedBytes > 0 ? new byte[preservedBytes] : null;
            }
        }

        protected virtual void OnWrappedAround()
        {
            WrappedAround?.Invoke(this, EventArgs.Empty);
        }

        public long WritePosition => _circularBuffer.WritePosition;
        public TimeSpan CurrentWriteTime => TimeSpan.FromSeconds((double)WritePosition / WaveFormat.AverageBytesPerSecond);

        #region Reimplementation of BufferedWaveProvider
        public bool DiscardOnBufferOverflow { get; set; }
        public bool ReadFully { get; set; }

        public int BufferLength { get; private set; }
        public TimeSpan BufferDuration
        {
            get { return TimeSpan.FromSeconds((double)BufferLength / WaveFormat.AverageBytesPerSecond); }
            private set { BufferLength = (int)(value.TotalSeconds * WaveFormat.AverageBytesPerSecond); }
        }

        public int AddSamples(byte[] buffer, int offset, int count)
        {
            var written = _circularBuffer.Write(buffer, offset, count);
            if (written < count && !DiscardOnBufferOverflow)
                throw new InvalidOperationException("Buffer full");
            return written;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _circularBuffer.Read(buffer, offset, count);
            if (ReadFully && read < count)
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