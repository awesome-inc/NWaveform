using System;
using System.Diagnostics;
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
        private readonly MenuViewModel _labelMenu;

        public string ToolTip => DisplayName;

        public PlayerViewModel(IWaveformPlayerViewModel audioPlayer)
        {
            if (audioPlayer == null) throw new ArgumentNullException(nameof(audioPlayer));
            AudioPlayer = audioPlayer;

            // ReSharper disable VirtualMemberCallInConstructor
            DisplayName = "Player";
            NotifyOfPropertyChange(nameof(ToolTip));
            // ReSharper restore VirtualMemberCallInConstructor

            _labelMenu = new MenuViewModel(new[]
            {
                new MenuItemViewModel
                {
                    Icon = IconChar.Pencil,
                    Command = new DelegateCommand<ILabelVievModel>(EditLabel)
                    {
                        Title = "Edit...",
                        Description = "Edit label"
                    }
                },
                new MenuItemViewModel
                {
                    Icon = IconChar.Times,
                    Command = new DelegateCommand<ILabelVievModel>(RemoveLabel)
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
                }
            });

            AudioPlayer.Waveform.SelectionMenu = selectionMenu;
        }

        protected override void OnDeactivate(bool close)
        {
            AudioPlayer?.Player?.Pause();
            base.OnDeactivate(close);
        }

        private bool CanSelectWaypoint(IAudioSelectionViewModel selection)
        {
            return (selection != null && selection.Duration > 1.0);
        }

        private void SelectWaypoint(IAudioSelectionViewModel selection)
        {
            MessageBox.Show("Assign waypoints...");
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

        // ReSharper disable once MemberCanBePrivate.Global
        public IWaveformPlayerViewModel AudioPlayer { get; private set; }

        // ReSharper disable once UnusedMember.Global
        public void OpenFile()
        {
            var dlg = new OpenFileDialog();
            var res = dlg.ShowDialog();

            if (res.HasValue && res.Value)
                OpenUrl(dlg.FileName);
        }

        // ReSharper disable once UnusedMember.Global
        public void OpenUrl()
        {
            var url = Interaction.InputBox("Url", "Open Url", "channel://1/");
            if (!string.IsNullOrEmpty(url))
                OpenUrl(url);
        }

        private void OpenUrl(string url)
        {
            try
            {
                AudioPlayer.Source = new Uri(url);
                //AddRandomProperties();
                DisplayName = url;
                NotifyOfPropertyChange(nameof(ToolTip));
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
            };

            AudioPlayer.Waveform.UserChannel = TestData.GetRandomWaypoints(r, duration, (int) (0.2 * duration), 0.5,
                0.85);

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
                    Menu = _labelMenu
                }
            };
        }
    }
}