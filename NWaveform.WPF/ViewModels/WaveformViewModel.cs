using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using NWaveform.Events;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    public class WaveformViewModel : WaveformDisplayViewModel, IWaveformViewModel
    {
        private PointCollection _userChannel = new PointCollection();
        private PointCollection _separationLeftChannel = new PointCollection();
        private PointCollection _separationRightChannel = new PointCollection();

        private readonly AudioSelectionViewModel _selection = new AudioSelectionViewModel();

        private double _ticksEach;

        private SolidColorBrush _positionBrush;
        private SolidColorBrush _selectionBrush;

        private readonly BindableCollection<ILabelVievModel> _labels = new BindableCollection<ILabelVievModel>();
        private ILabelVievModel _selectedLabel;
        private IMenuViewModel _selectionMenu;
        private SolidColorBrush _userBrush;
        private SolidColorBrush _separationLeftBrush;
        private SolidColorBrush _separationRightBrush;
        private SolidColorBrush _userTextBrush;
        private IPositionProvider _positionProvider = new EmptyPositionProvider();
        private IMediaPlayer _player;
        private bool _flagAutoPlay;

        public WaveformViewModel(IEventAggregator events, IGetWaveform getWaveform, WaveformSettings waveformSettings = null) : base(events, getWaveform)
        {
            var settings = waveformSettings ?? new WaveformSettings();

            _ticksEach = settings.TicksEach;
            BackgroundBrush = new SolidColorBrush(settings.BackgroundColor);
            LeftBrush = new SolidColorBrush(settings.LeftColor);
            RightBrush = new SolidColorBrush(settings.RightColor);
            _positionBrush = new SolidColorBrush(settings.PositionColor);
            _selectionBrush = new SolidColorBrush(settings.SelectionColor);
            _userBrush = new SolidColorBrush(settings.UserColor);
            _separationLeftBrush = new SolidColorBrush(settings.SeparationLeftColor);
            _separationRightBrush = new SolidColorBrush(settings.SeparationRightColor);
            _userTextBrush = new SolidColorBrush(settings.UserTextColor);
            LastWriteBrush = new SolidColorBrush(settings.LastWriteColor);
            LiveDelta = settings.LiveDelta;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            SelectionMenu = null;
            Selection = null;
            PositionProvider = null;
            SelectedLabel = null;
            Labels.Clear();
            UserChannel = null;
            SeparationLeftChannel = null;
            SeparationRightChannel = null;
        }

        public double LiveDelta { get; }

        public IPositionProvider PositionProvider
        {
            get => _positionProvider;
            set
            {
                if (Equals(value, _positionProvider)) return;
                _positionProvider.PropertyChanged -= PositionProviderNotifyOfPropertyChange;
                _positionProvider = value ?? new EmptyPositionProvider();
                _positionProvider.PropertyChanged += PositionProviderNotifyOfPropertyChange;
                _player = _positionProvider as IMediaPlayer;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Position));
                Source = _positionProvider.Source;
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            _flagAutoPlay = true;
        }

        protected internal override void HandlePeaks(PeaksReceivedEvent message)
        {
            base.HandlePeaks(message);
            if (IsActive && _flagAutoPlay && IsLive)
                CheckAutoPlay();
        }

        private void CheckAutoPlay()
        {
            if (_player == null || !_player.CanPlay) return;
            _player.Position = LastWritePosition - LiveDelta;
            _player.Play();
            _flagAutoPlay = false;
        }

        protected internal override void HandleShift(double shift)
        {
            base.HandleShift(shift);

            NotifyOfPropertyChange(nameof(Position));

            if (Selection != null)
            {
                Selection.Start -= shift;
                Selection.End -= shift;
            }

            ShiftChannels(shift);
            ShiftLabels(shift);
        }

        private void ShiftChannels(double shift)
        {
            SeparationLeftChannel = SeparationLeftChannel.Shifted(shift, Duration);
            SeparationRightChannel = SeparationRightChannel.Shifted(shift, Duration);
            UserChannel = UserChannel.Shifted(shift, Duration);
        }

        private void ShiftLabels(double shift)
        {
            _labels.Apply(l => l.Position -= shift);
            _labels.RemoveRange(_labels.Where(l => l.Position < 0 || l.Position >= Duration).ToList());
        }

        private void PositionProviderNotifyOfPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            // Position if a direct reference to _positionProvider whereby Duration caches the value in this instance
            switch (e.PropertyName)
            {
                case nameof(IPositionProvider.Position):
                    NotifyOfPropertyChange(nameof(Position));
                    break;
                case nameof(IPositionProvider.Duration):
                    Duration = PositionProvider.Duration;
                    break;
                case nameof(IPositionProvider.Source):
                    Source = PositionProvider.Source;
                    if (Source != null) _flagAutoPlay = true;
                    break;
            }
        }

        public double Position
        {
            get => PositionProvider.Position;
            set
            {
                var pos = Position;
                if (Math.Abs(pos - value) < double.Epsilon) return;
                PositionProvider.Position = value;
                NotifyOfPropertyChange();
            }
        }

        public IAudioSelectionViewModel Selection
        {
            get => _selection;
            set
            {

                _selection.Copy(value);
                PositionProvider.AudioSelection = _selection.IsEmpty() ? AudioSelection.Empty : new AudioSelection(0, _selection.Start, _selection.End);
                NotifyOfPropertyChange();
            }
        }

        protected override void OnSourceChanged()
        {
            _selection.Source = Source;
        }


        public IMenuViewModel SelectionMenu
        {
            get => _selectionMenu;
            set { _selectionMenu = value; NotifyOfPropertyChange(); }
        }

        public double TicksEach
        {
            get => _ticksEach;
            set { _ticksEach = value; NotifyOfPropertyChange(); }
        }

        public IList<ILabelVievModel> Labels
        {
            get => _labels;
            set
            {
                _labels.Clear();
                if (value != null)
                    _labels.AddRange(value);
            }
        }

        public ILabelVievModel SelectedLabel
        {
            get => _selectedLabel;
            set { _selectedLabel = value; NotifyOfPropertyChange(); }
        }

        public SolidColorBrush UserBrush
        {
            get => _userBrush;
            set { _userBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public SolidColorBrush SeparationLeftBrush
        {
            get => _separationLeftBrush;
            set { _separationLeftBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public SolidColorBrush SeparationRightBrush
        {
            get => _separationRightBrush;
            set { _separationRightBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public SolidColorBrush UserTextBrush
        {
            get => _userTextBrush;
            set { _userTextBrush = value; NotifyOfPropertyChange(); }
        }

        public SolidColorBrush PositionBrush
        {
            get => _positionBrush;
            set { _positionBrush = value; NotifyOfPropertyChange(); }
        }

        public SolidColorBrush SelectionBrush
        {
            get => _selectionBrush;
            set { _selectionBrush = value; NotifyOfPropertyChange(); }
        }

        public PointCollection UserChannel
        {
            get => _userChannel;
            set { _userChannel = value; NotifyOfPropertyChange(); }
        }

        public PointCollection SeparationLeftChannel
        {
            get => _separationLeftChannel;
            set { _separationLeftChannel = value; NotifyOfPropertyChange(); }
        }

        public PointCollection SeparationRightChannel
        {
            get => _separationRightChannel;
            set { _separationRightChannel = value; NotifyOfPropertyChange(); }
        }
    }
}
