using System;
using System.Diagnostics;

namespace NWaveform.NAudio
{
    public class SeekableCircularBuffer
    {
        private readonly byte[] _buffer;
        private readonly object _lockObject = new object();
        private int _readPosition;

        public int ReadPosition
        {
            get { return _readPosition; }
            set
            {
                lock (_lockObject)
                {
                    _readPosition = Math.Min(value, Math.Max(0, _buffer.Length));
                }
            }
        }

        public int WritePosition { get; private set; }

        public SeekableCircularBuffer(int size)
        {
            _buffer = new byte[size];
        }

        public int Write(byte[] data, int offset, int count)
        {
            lock (_lockObject)
            {
                var bytesWritten = 0;
                // write to end
                var writeToEnd = Math.Min(_buffer.Length - WritePosition, count);
                Array.Copy(data, offset, _buffer, WritePosition, writeToEnd);
                WritePosition += writeToEnd;
                WritePosition %= _buffer.Length;
                bytesWritten += writeToEnd;
                if (bytesWritten < count)
                {
                    Debug.Assert(WritePosition == 0);
                    // must have wrapped round. Write to start
                    Array.Copy(data, offset + bytesWritten, _buffer, WritePosition, count - bytesWritten);
                    WritePosition += (count - bytesWritten);
                    bytesWritten = count;
                }
                return bytesWritten;
            }
        }

        public int Read(byte[] data, int offset, int count)
        {
            lock (_lockObject)
            {
                var bytesRead = 0;
                var readToEnd = Math.Min(_buffer.Length - _readPosition, count);
                Array.Copy(_buffer, _readPosition, data, offset, readToEnd);
                bytesRead += readToEnd;
                _readPosition += readToEnd;
                _readPosition %= _buffer.Length;

                if (bytesRead < count)
                {
                    // must have wrapped round. Read from start
                    Debug.Assert(_readPosition == 0);
                    Array.Copy(_buffer, _readPosition, data, offset + bytesRead, count - bytesRead);
                    _readPosition += (count - bytesRead);
                    bytesRead = count;
                }

                return bytesRead;
            }
        }

        public void Shift(int from, int to, int count)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (from < 0 || from + count > _buffer.Length) throw new ArgumentOutOfRangeException(nameof(from));
            if (to < 0 || to + count > _buffer.Length) throw new ArgumentOutOfRangeException(nameof(to));

            lock (_lockObject)
            {
                Buffer.BlockCopy(_buffer, from, _buffer, to, count);
                var delta = to - from;
                _readPosition = Mod(_readPosition + delta, _buffer.Length);
                WritePosition = Mod(WritePosition + delta, _buffer.Length);
            }
        }

        private static int Mod(int n, int m)
        {
            return (n + m) % m;
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                _readPosition = 0;
                WritePosition = 0;
                Array.Clear(_buffer, 0, _buffer.Length);
            }
        }
    }
}