using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Caliburn.Micro;
using FontAwesome.Sharp;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using NWaveform.ViewModels;

namespace NWaveform.App
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerViewModel : Screen, IPlayerViewModel
    {
        private readonly IEventAggregator _events;
        private readonly MenuViewModel _labelMenu;

        public string ToolTip => DisplayName;

        public Uri Source
        {
            get => AudioPlayer?.Source;
            set
            {
                AudioPlayer.Source = value;
                DisplayName = value?.ToString();
                NotifyOfPropertyChange(nameof(ToolTip));
                if (value != null) AddRandomProperties();
            }
        }

        public void Handle(ActivateChannel message)
        {
            if (!IsActive) return;
            OpenUrl(message.Source);
        }

        public PlayerViewModel(IEventAggregator events, IWaveformPlayerViewModel audioPlayer)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            AudioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
            AudioPlayer.ConductWith(this);

            // ReSharper disable VirtualMemberCallInConstructor
            DisplayName = "Player";
            NotifyOfPropertyChange(nameof(ToolTip));
            // ReSharper restore VirtualMemberCallInConstructor

            _labelMenu = new MenuViewModel(new[]
            {
                new MenuItemViewModel
                {
                    Icon = IconChar.Pencil,
                    Command = new DelegateCommand<ILabelVievModel>(EditLabel, l => false)
                    {
                        Title = "Edit...",
                        Description = "Edit label"
                    }
                },
                new MenuItemViewModel
                {
                    Icon = IconChar.Times,
                    Command = new DelegateCommand<ILabelVievModel>(RemoveLabel, l => false)
                    {
                        Title = "Remove",
                        Description = "Remove label"
                    }
                }
            });

            var selectionMenu = new MenuViewModel(new[]
            {
                new MenuItemViewModel
                {
                    Icon = IconChar.LocationArrow,
                    Command = new DelegateCommand<IAudioSelectionViewModel>(SelectWaypoint, CanSelectWaypoint)
                    {
                        Title = "Assign WP to...",
                        Description = "Assign a WP from the selection to..."
                    }
                },
                new MenuItemViewModel
                {
                    Icon = IconChar.Scissors,
                    Command = new DelegateCommand<IAudioSelectionViewModel>(CropToFile, CanCropToFile)
                    {
                        Title = "Crop to file...",
                        Description = "Save the current audio selection to file"
                    }
                }
            });

            AudioPlayer.Waveform.SelectionMenu = selectionMenu;

            _events.Subscribe(this);
        }

        private bool CanSelectWaypoint(IAudioSelectionViewModel selection)
        {
            return (selection != null && selection.Duration > 1.0);
        }

        private void SelectWaypoint(IAudioSelectionViewModel selection)
        {
            MessageBox.Show("Assign waypoints...");
        }

        private bool CanCropToFile(IAudioSelectionViewModel selection)
        {
            return (selection != null && selection.Duration > 1.0);
        }

        private void CropToFile(IAudioSelectionViewModel selection)
        {
            var message = new CropAudioRequest(selection);
            _events.PublishOnUIThread(message);
        }

        private void EditLabel(ILabelVievModel label)
        {
            MessageBox.Show("Edit Label...");
        }

        private void RemoveLabel(ILabelVievModel label)
        {
            var waveForm = AudioPlayer.Waveform;
            Debug.Assert(label == waveForm.SelectedLabel);
            waveForm.Labels.Remove(label);
        }

        public IWaveformPlayerViewModel AudioPlayer { get; }

        public void OpenFile()
        {
            var dlg = new OpenFileDialog
            {
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"),
                FileName = "Pulp_Fiction_Jimmys_Coffee.mp3"
            };
            var res = dlg.ShowDialog();

            if (res.HasValue && res.Value)
                OpenUrl(dlg.FileName);
        }

        // ReSharper disable once UnusedMember.Global
        public void OpenUrl()
        {
            var url = Interaction.InputBox("Url", "Open Url", "channel://1/ or http://rad.reaper.fm/stream.php");
            if (!string.IsNullOrEmpty(url))
                OpenUrl(url);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Source = new Uri(url);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Could not open url: {0}", ex);
                MessageBox.Show("Could not open url: " + ex.Message);
                DisplayName = "Player";
                NotifyOfPropertyChange(nameof(ToolTip));
            }
        }

        private void AddRandomProperties()
        {
            var duration = AudioPlayer.Player.Duration;
            var r = new Random();

            // randomly set position
            AudioPlayer.Player.Position = r.NextDouble() * duration;

            // randomly set selection
            var a = r.NextDouble() * duration;
            var b = r.NextDouble() * duration;
            AudioPlayer.Waveform.Selection = new AudioSelectionViewModel
            {
                Start = Math.Min(a, b),
                End = Math.Max(a, b),
                Source = AudioPlayer.Source
            };

            AudioPlayer.Waveform.UserChannel = TestData.GetRandomWaypoints(r, duration, (int)(0.2 * duration), 0.5, 0.85);

            AudioPlayer.Waveform.SeparationLeftChannel = TestData.GetSeparationTop(r, duration, 10);
            AudioPlayer.Waveform.SeparationRightChannel = TestData.GetSeparationBottom(r, duration, 8);

            AudioPlayer.Waveform.Labels = new ILabelVievModel[]
            {
                new LabelVievModel
                {
                    Background = AudioPlayer.Waveform.UserBrush,
                    Foreground = AudioPlayer.Waveform.UserTextBrush,
                    Text = "Waypoint",
                    Icon = IconChar.LocationArrow,
                    IconRotation = r.NextDouble() * 360,
                    IconFlipOrientation = (FlipOrientation)r.Next((int)FlipOrientation.Vertical + 1),
                    IconSpin = true,
                    Menu = _labelMenu
                }
            };
        }
    }
}
