using System;
using System.IO;

namespace NWaveform.App
{
    public class ReadFullyStream : Stream
    {
        private readonly Stream _sourceStream;
        private long _pos; // psuedo-position
        private readonly byte[] _readAheadBuffer;
        private int _readAheadLength;
        private int _readAheadOffset;

        public ReadFullyStream(Stream sourceStream)
        {
            _sourceStream = sourceStream;
            _readAheadBuffer = new byte[4096];
        }

        public override void Close()
        {
            _sourceStream.Close();
            base.Close();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public override void Flush() { throw new InvalidOperationException(); }

        public override long Length => _pos;

        public override long Position
        {
            get => _pos;
            set => throw new InvalidOperationException();
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = 0;
            while (bytesRead < count)
            {
                var readAheadAvailableBytes = _readAheadLength - _readAheadOffset;
                var bytesRequired = count - bytesRead;
                if (readAheadAvailableBytes > 0)
                {
                    var toCopy = Math.Min(readAheadAvailableBytes, bytesRequired);
                    Array.Copy(_readAheadBuffer, _readAheadOffset, buffer, offset + bytesRead, toCopy);
                    bytesRead += toCopy;
                    _readAheadOffset += toCopy;
                }
                else
                {
                    _readAheadOffset = 0;
                    _readAheadLength = _sourceStream.Read(_readAheadBuffer, 0, _readAheadBuffer.Length);
                    //Debug.WriteLine(String.Format("Read {0} bytes (requested {1})", readAheadLength, readAheadBuffer.Length));
                    if (_readAheadLength == 0)
                    {
                        break;
                    }
                }
            }
            _pos += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }
    }
}
