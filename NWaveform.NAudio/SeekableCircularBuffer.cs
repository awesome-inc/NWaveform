using System;
using System.Diagnostics;

namespace NWaveform.NAudio
{
    public class SeekableCircularBuffer
    {
        private readonly byte[] _buffer;
        private readonly object _lockObject = new object();
        private int _writePosition;
        private int _byteCount;
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

        public SeekableCircularBuffer(int size)
        {
            _buffer = new byte[size];
        }

        public int Write(byte[] data, int offset, int count)
        {
            lock (_lockObject)
            {
                var bytesWritten = 0;
                if (count > _buffer.Length - _byteCount)
                {
                    count = _buffer.Length - _byteCount;
                }
                // write to end
                int writeToEnd = Math.Min(_buffer.Length - _writePosition, count);
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
                _byteCount += bytesWritten;
                return bytesWritten;
            }
        }

        public int Read(byte[] data, int offset, int count)
        {
            lock (_lockObject)
            {
                if (count > _byteCount)
                {
                    count = _byteCount;
                }
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

                _byteCount -= bytesRead;
                Debug.Assert(_byteCount >= 0);
                return bytesRead;
            }
        }

        public int MaxLength => _buffer.Length;

        public int Count
        {
            get
            {
                lock (_lockObject)
                {
                    return _byteCount;
                }
            }
        }

        public void Reset()
        {
            lock (_lockObject)
            {
                ResetInner();
            }
        }

        private void ResetInner()
        {
            _byteCount = 0;
            _readPosition = 0;
            _writePosition = 0;
        }

        public void Advance(int count)
        {
            lock (_lockObject)
            {
                if (count >= _byteCount)
                {
                    ResetInner();
                }
                else
                {
                    _byteCount -= count;
                    _readPosition += count;
                    _readPosition %= MaxLength;
                }
            }
        }
    }
}