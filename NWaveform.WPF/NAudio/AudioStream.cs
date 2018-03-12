using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NWaveform.Extensions;

namespace NWaveform.NAudio
{
    /// <summary>
    /// Simply a copy & paste from NAudios <see cref="AudioFileReader"/> so we can override the 
    /// logic for creating reader streams and enable uri support.
    /// Since most members in <see cref="AudioFileReader"/> is private we cannot subclass but
    /// must re-implement via copy & paste.
    /// </summary>
    internal class AudioStream : WaveStream, ISampleProvider
    {
        private WaveStream _readerStream; // the waveStream which we will use for all positioning
        private readonly SampleChannel _sampleChannel; // sample provider that gives us most stuff we need
        private readonly int _destBytesPerSample;
        private readonly int _sourceBytesPerSample;
        private readonly long _length;
        private readonly object _lockObject;

        /// <summary>
        /// Initializes a new instance of AudioStream
        /// </summary>
        /// <param name="source">The media source to open</param>
        public AudioStream(Uri source)
        {
            _lockObject = new object();
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            CreateReaderStream(source);
            _sourceBytesPerSample = (_readerStream.WaveFormat.BitsPerSample / 8) * _readerStream.WaveFormat.Channels;
            _sampleChannel = new SampleChannel(_readerStream, false);
            _destBytesPerSample = 4*_sampleChannel.WaveFormat.Channels;
            _length = SourceToDest(_readerStream.Length);
        }

        /// <summary>
        /// Creates the reader stream, supporting all filetypes in the core NAudio library,
        /// and ensuring we are in PCM format.
        /// </summary>
        /// <param name="source">Source uri</param>
        protected virtual void CreateReaderStream(Uri source)
        {
            var fileName = source.GetFileName(true);
            if (!string.IsNullOrEmpty(fileName))
                CreateReaderStream(fileName);
            else
                // for remote uris fall back to media foundation reader, see if that can play it
                _readerStream = new MediaFoundationReader(source.ToString());
        }

        private void CreateReaderStream(string fileName)
        {
            if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                _readerStream = new WaveFileReader(fileName);
                if (_readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm &&
                    _readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                {
                    _readerStream = WaveFormatConversionStream.CreatePcmStream(_readerStream);
                    _readerStream = new BlockAlignReductionStream(_readerStream);
                }
            }
            else if (fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                _readerStream = new Mp3FileReader(fileName);
            }
            else if (fileName.EndsWith(".aiff"))
            {
                _readerStream = new AiffFileReader(fileName);
            }
            else
            {
                // fall back to media foundation reader, see if that can play it
                _readerStream = new MediaFoundationReader(fileName);
            }
        }

        /// <summary>
        /// WaveFormat of this stream
        /// </summary>
        public override WaveFormat WaveFormat => _sampleChannel.WaveFormat;

        /// <summary>
        /// Length of this stream (in bytes)
        /// </summary>
        public override long Length => _length;

        /// <summary>
        /// Position of this stream (in bytes)
        /// </summary>
        public override long Position
        {
            get => SourceToDest(_readerStream.Position);
            set { lock (_lockObject) { _readerStream.Position = DestToSource(value); }  }
        }

        /// <summary>
        /// Reads from this wave stream
        /// </summary>
        /// <param name="buffer">Audio buffer</param>
        /// <param name="offset">Offset into buffer</param>
        /// <param name="count">Number of bytes required</param>
        /// <returns>Number of bytes read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var waveBuffer = new WaveBuffer(buffer);
            int samplesRequired = count / 4;
            int samplesRead = Read(waveBuffer.FloatBuffer, offset / 4, samplesRequired);
            return samplesRead * 4;
        }

        /// <summary>
        /// Reads audio from this sample provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="count">Number of samples required</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            lock (_lockObject)
            {
                return _sampleChannel.Read(buffer, offset, count);
            }
        }

        /// <summary>
        /// Gets or Sets the Volume of this AudioFileReader. 1.0f is full volume
        /// </summary>
        public float Volume
        {
            get => _sampleChannel.Volume;
            set => _sampleChannel.Volume = value;
        }

        /// <summary>
        /// Helper to convert source to dest bytes
        /// </summary>
        private long SourceToDest(long sourceBytes)
        {
            return _destBytesPerSample * (sourceBytes / _sourceBytesPerSample);
        }

        /// <summary>
        /// Helper to convert dest to source bytes
        /// </summary>
        private long DestToSource(long destBytes)
        {
            return _sourceBytesPerSample * (destBytes / _destBytesPerSample);
        }

        /// <summary>
        /// Disposes this AudioFileReader
        /// </summary>
        /// <param name="disposing">True if called from Dispose</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _readerStream.Dispose();
                _readerStream = null;
            }
            base.Dispose(disposing);
        }
    }

}
