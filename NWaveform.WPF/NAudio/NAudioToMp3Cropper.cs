using System;
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
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _events.Subscribe(this);
        }

        public async Task Handle(CropAudioRequest message)
        {
            var fileName = message.OutputFilename ?? GetFilename();
            if (string.IsNullOrWhiteSpace(fileName)) return;
            await Task.Factory.StartNew(() => CropToFile(message, fileName));
        }

        private void CropToFile(CropAudioRequest message, string fileName)
        {
            var reader = _factory.Create(message.Selection.Source);
            using (reader as IDisposable)
            using (var writer = new LameMP3FileWriter(fileName, reader.WaveFormat, BitRate))
            {
                var inputLength = (int) (message.Selection.Duration * reader.WaveFormat.AverageBytesPerSecond);
                var bytesLeft = inputLength;
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

        private static string GetFilename()
        {
            var dlg = new SaveFileDialog
            {
                //InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"),
                FileName = "selection.mp3",
                Filter = "MP3 audio files (*.mp3)|*.mp3"
            };
            var res = dlg.ShowDialog();
            return res.HasValue && res.Value ? dlg.FileName : string.Empty;
        }
    }
}
