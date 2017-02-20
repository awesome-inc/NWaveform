using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.Win32;
using NAudio.Lame;

namespace NWaveform.NAudio
{
    public class NAudioToMp3Cropper : IHandleWithTask<CropAudioRequest>
    {
        private readonly IEventAggregator _events;
        private readonly IWaveProviderFactory _factory;

        public int BitRate { get; set; } = 128;

        public NAudioToMp3Cropper(IEventAggregator events, IWaveProviderFactory factory)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _events = events;
            _factory = factory;
            _events.Subscribe(this);
        }

        public Task Handle(CropAudioRequest message)
        {
            var fileName = message.OutputFilename ?? GetFilename();
            if (string.IsNullOrWhiteSpace(fileName)) return Task.FromResult(0);
            return Task.Factory.StartNew(() => CropToFile(message, fileName));
        }

        private void CropToFile(CropAudioRequest message, string fileName)
        {
            var reader = _factory.Create(message.Source);
            using (reader as IDisposable)
            using (var writer = new LameMP3FileWriter(fileName, reader.WaveFormat, BitRate))
            {
                var inputLength = (int) (message.Selection.Duration * reader.WaveFormat.AverageBytesPerSecond);
                var bytesLeft = inputLength;

                //writer.MinProgressTime = 500;
                writer.OnProgress += (o, inputBytes, outputBytes, finished) =>
                {
                    var msg = FormatProgress(inputBytes, inputLength, outputBytes);
                    Trace.WriteLine(msg);
                };

                var buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
                reader.CurrentTime = TimeSpan.FromSeconds(message.Selection.Start);
                while (bytesLeft > 0)
                {
                    var bytesRead = reader.Read(buffer, 0, Math.Min(buffer.Length, bytesLeft));
                    writer.Write(buffer, 0, bytesRead);
                    bytesLeft -= bytesRead;
                }
            }

            _events.PublishOnUIThread(new CropAudioResponse(new Uri(fileName)));
        }

        private static string FormatProgress(long inputBytes, int inputLength, long outputBytes)
        {
            string msg =
                $"Progress: {(inputBytes * 100.0) / inputLength:0.0}%, Output: {outputBytes:#,0} bytes, Ratio: 1:{((double) inputBytes) / Math.Max(1, outputBytes):0.0}";
            return msg;
        }

        private static string GetFilename()
        {
            var dlg = new SaveFileDialog
            {
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"),
                FileName = "selection.mp3"
            };
            var res = dlg.ShowDialog();
            return res.HasValue && res.Value ? dlg.FileName : string.Empty;
        }
    }
}