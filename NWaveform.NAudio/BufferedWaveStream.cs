using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class BufferedWaveStream : WaveStream, IWaveProviderEx
    {
        public override WaveFormat WaveFormat { get; }
        private readonly SeekableCircularBuffer _circularBuffer;

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
            set { _circularBuffer.ReadPosition = (int) value;}
        }
        #endregion

        #region IWaveProviderEx
        public override TimeSpan TotalTime => BufferDuration;
        public float Volume { get; set; }
        public float Pan { get; set; }
        public bool SupportsPanning => false;
        #endregion

        #region Reimplementation of BufferedWaveProvider
        public bool DiscardOnBufferOverflow { get; set; }
        public bool ReadFully { get; set; }

        public int BufferedBytes => _circularBuffer?.Count ?? 0;
        public int BufferLength { get; private set; }
        public TimeSpan BufferedDuration => TimeSpan.FromSeconds((double)BufferedBytes / WaveFormat.AverageBytesPerSecond);
        public TimeSpan BufferDuration
        {
            get
            {
                return TimeSpan.FromSeconds((double)BufferLength / WaveFormat.AverageBytesPerSecond);
            }
            private set
            {
                BufferLength = (int)(value.TotalSeconds * WaveFormat.AverageBytesPerSecond);
            }
        }

        public void AddSamples(byte[] buffer, int offset, int count)
        {
            var written = _circularBuffer.Write(buffer, offset, count);
            if (written < count && !DiscardOnBufferOverflow)
            {
                throw new InvalidOperationException("Buffer full");
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            if (_circularBuffer != null) // not yet created
            {
                read = _circularBuffer.Read(buffer, offset, count);
            }
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
            _circularBuffer?.Reset();
        }

        #endregion
    }
}