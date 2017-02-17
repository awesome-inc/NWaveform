using System;
using System.Diagnostics;

namespace NWaveform.NAudio
{
    public class SeekableCircularBuffer
    {
        private readonly byte[] _buffer;
        private readonly object _lockObject = new object();
        private int _writePosition;
        private int _readPosition;

        public int ReadPosition
        {
            get { return _readPosition; }
            set
            {
                lock (_lockObject)
                {
                    _readPosition = Math.Min(value, _buffer.Length);
                }
            }
        }

        public int WritePosition => _writePosition;

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
                var writeToEnd = Math.Min(_buffer.Length - _writePosition, count);
                Array.Copy(data, offset, _buffer, _writePosition, writeToEnd);
                _writePosition += writeToEnd;
                _writePosition %= _buffer.Length;
                bytesWritten += writeToEnd;
                if (bytesWritten < count)
                {
                    Debug.Assert(_writePosition == 0);
                    // must have wrapped round. Write to start
                    Array.Copy(data, offset + bytesWritten, _buffer, _writePosition, count - bytesWritten);
                    _writePosition += (count - bytesWritten);
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

        public void Clear()
        {
            lock (_lockObject)
            {
                _readPosition = 0;
                _writePosition = 0;
                Array.Clear(_buffer, 0, _buffer.Length);
            }
        }
    }
}