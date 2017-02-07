using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using NWaveform.Extensions;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.Serializer
{
    public class WaveFormSerializer : IWaveFormSerializer
    {
        public WaveformData Read(string source)
        {
            var fmt = (FileFormat)FindExtension(source, Extensions);
            switch (fmt)
            {
                // zipped file
                case FileFormat.Zipped:
                    using (var zipStream = CreateZipInputStream(source))
                    {
                        var zipEntry = zipStream.Entries.Single(); // open first (and only!) file in zip
                        using (var entryStream = zipEntry.Open())
                            return Read(entryStream);
                    }
                default:
                    using (var fileStream = File.Open(source, FileMode.Open, FileAccess.Read))
                        return Read(fileStream);
            }
        }

        public void Save(Stream stream, WaveformData waveformData)
        {
            using (var zipStream = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                var zipEntry = zipStream.CreateEntry("WaveformData.twb");
                using (var entryStream = zipEntry.Open())
                    Write(entryStream, waveformData);
            }
        }


        public string GetWaveFormUri(string strUri)
        {
            // NOTE: Methods from System.IO.Path will not work here!
            if (String.IsNullOrEmpty(strUri))
                throw new ArgumentNullException(nameof(strUri));

            var p = strUri.LastIndexOf('.');
            if (p == -1 || // not found
                strUri.Length - p > 5) // file extension should part have at most 5 chars
                throw new ArgumentException(@"Not a file based Uri, since it has no file extension part.", nameof(strUri));

            return strUri.Substring(0, p) + DefaultExtension;
        }

        #region Private

        private static WaveformData Read(Stream inputStream)
        {
            using (var reader = new BinaryReader(inputStream))
                return Read(reader);
        }

        private static void Write(Stream outputStream, WaveformData waveformData)
        {
            using (var writer = new BinaryWriter(outputStream))
                Write(writer, waveformData);
        }

        private enum FileFormat { Zipped = 0 }

        private static readonly string[] Extensions = { ".twz", ".twb" };
        private static string DefaultExtension => GetExtension(FileFormat.Zipped);

        private static string GetExtension(FileFormat format) { return Extensions[(int)format]; }

        private static int FindExtension(string source, params string[] ext)
        {
            try
            {
                var fileExt = Path.GetExtension(source);
                for (int i = 0; i < ext.Length; i++)
                {
                    if (String.Equals(fileExt, ext[i], StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Could not get file extension: {0}", ex);
            }
            return -1;
        }

        private static WaveformData Read(BinaryReader reader)
        {
            var waveForm = new WaveformData
            {
                Source = reader.ReadString(),
                Description = reader.ReadBoolean() ? reader.ReadString() : String.Empty,
                Duration = TimeSpan.FromSeconds(reader.ReadDouble()),
                SampleRate = reader.ReadInt32(),
                Channels = new Channel[reader.ReadByte()]
            };

            for (int i = 0; i < waveForm.Channels.Length; i++)
            {
                waveForm.Channels[i] = new Channel();
                var samples = ReadValArray<float>(reader);
                if (samples != null && samples.Length > 0)
                    waveForm.Channels[i].Samples = samples;
            }

            return waveForm;
        }

        private static void Write(BinaryWriter writer, WaveformData waveformData)
        {
            writer.Write(waveformData.Source);
            var description = waveformData.Description ?? String.Empty;
            writer.Write(!String.IsNullOrEmpty(description));
            writer.Write(description);
            writer.Write(waveformData.Duration.TotalSeconds);
            writer.Write(waveformData.SampleRate);

            writer.Write((byte)waveformData.Channels.Length);
            foreach (var channel in waveformData.Channels)
                WriteValArray(channel.Samples, writer);
        }

        private static ZipArchive CreateZipInputStream(string source)
        {
            var uri = new Uri(source);
            var fileName = uri.GetFileName(true);
            if (!String.IsNullOrEmpty(fileName))
            {
                var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                return new ZipArchive(fileStream);
            }

            if (uri.Scheme != "http")
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Unsupported uri scheme: \"{0}\".", uri.Scheme));

            uri.VerifyUriExists();

            using (var webClient = new WebClient())
            {
                var data = webClient.DownloadData(source);
                var memoryStream = new MemoryStream(data);
                return new ZipArchive(memoryStream);
            }
        }

        private static T[] FromByteArray<T>(byte[] values)
        {
            Debug.Assert(values != null && values.Length > 0);
            var size = Marshal.SizeOf(typeof(T));
            var len = values.Length / Marshal.SizeOf(typeof(T));
            var ret = new T[len];
            unsafe
            {
                for (var i = 0; i < ret.Length; i++)
                {
                    fixed (byte* b = &values[i * size])
                    {
                        var ptr = new IntPtr(b);
                        ret[i] = (T)Marshal.PtrToStructure(ptr, typeof(T));
                    }
                }
            }
            return ret;
        }

        private static byte[] ToByteArray<T>(T[] values)
        {
            Debug.Assert(values != null && values.Length > 0);
            var size = Marshal.SizeOf(typeof(T));
            var len = values.Length * Marshal.SizeOf(typeof(T));
            var ret = new byte[len];

            unsafe
            {
                for (int i = 0; i < values.Length; i++)
                {
                    fixed (byte* b = &ret[i * size])
                    {
                        var ptr = new IntPtr(b);
                        Marshal.StructureToPtr(values[i], ptr, false);
                    }
                }
            }
            return ret;
        }

        private static T[] ReadValArray<T>(BinaryReader reader)
        {
            var numBytes = reader.ReadInt32();
            if (numBytes > 0)
            {
                var buffer = reader.ReadBytes(numBytes);
                var result = FromByteArray<T>(buffer);
                return result;
            }
            return numBytes < 0 ? null : new T[0];
        }

        private static void WriteValArray<T>(T[] values, BinaryWriter writer)
        {
            if (values == null)
                writer.Write(-1);
            else
            {
                var len = values.Length * Marshal.SizeOf(typeof(T));
                writer.Write(len);
                if (len <= 0) return;
                var buffer = ToByteArray(values);
                writer.Write(buffer);
            }
        }

        #endregion
    }
}