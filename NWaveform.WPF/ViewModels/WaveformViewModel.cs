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

        private IAudioSelectionViewModel _selection = new AudioSelectionViewModel();

        private readonly double _maxMagnitude;
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

        public WaveformViewModel(IEventAggregator events, IGetWaveform getWaveform, WaveformSettings waveformSettings = null) : base(events, getWaveform)
        {
            var settings = waveformSettings ?? new WaveformSettings();

            _maxMagnitude = settings.MaxMagnitude;
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
        }

        public IPositionProvider PositionProvider
        {
            get { return _positionProvider; }
            set
            {
                if (Equals(value, _positionProvider)) return;
                _positionProvider.PropertyChanged -= PositionProviderNotifyOfPropertyChange;
                _positionProvider = value ?? new EmptyPositionProvider();
                _positionProvider.PropertyChanged += PositionProviderNotifyOfPropertyChange;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Position));
            }
        }

        protected internal override void HandleShift(double shift)
        {
            base.HandleShift(shift);

            Position -= shift;

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
                    break;
            }
        }

        public double Position
        {
            get { return PositionProvider.Position; }
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
            get { return _selection; }
            set
            {
                _selection = value;
                // TODO: channel from top/height?!
                PositionProvider.AudioSelection = new AudioSelection(0, _selection.Start, _selection.End);
                NotifyOfPropertyChange();
            }
        }

        public IMenuViewModel SelectionMenu
        {
            get { return _selectionMenu; }
            set { _selectionMenu = value; NotifyOfPropertyChange(); }
        }

        public double TicksEach
        {
            get { return _ticksEach; }
            set { _ticksEach = value; NotifyOfPropertyChange(); }
        }

        public IList<ILabelVievModel> Labels
        {
            get { return _labels; }
            set
            {
                _labels.Clear();
                if (value != null)
                    _labels.AddRange(value);
            }
        }

        public ILabelVievModel SelectedLabel
        {
            get { return _selectedLabel; }
            set { _selectedLabel = value; NotifyOfPropertyChange(); }
        }

        public SolidColorBrush UserBrush
        {
            get { return _userBrush; }
            set { _userBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public SolidColorBrush SeparationLeftBrush
        {
            get { return _separationLeftBrush; }
            set { _separationLeftBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public SolidColorBrush SeparationRightBrush
        {
            get { return _separationRightBrush; }
            set { _separationRightBrush = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public SolidColorBrush UserTextBrush
        {
            get { return _userTextBrush; }
            set { _userTextBrush = value; NotifyOfPropertyChange(); }
        }

        public SolidColorBrush PositionBrush
        {
            get { return _positionBrush; }
            set { _positionBrush = value; NotifyOfPropertyChange(); }
        }

        public SolidColorBrush SelectionBrush
        {
            get { return _selectionBrush; }
            set { _selectionBrush = value; NotifyOfPropertyChange(); }
        }

        public PointCollection UserChannel
        {
            get { return _userChannel; }
            set { _userChannel = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public PointCollection SeparationLeftChannel
        {
            get { return _separationLeftChannel; }
            set { _separationLeftChannel = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public PointCollection SeparationRightChannel
        {
            get { return _separationRightChannel; }
            set { _separationRightChannel = value; NotifyOfPropertyChange(); RenderWaveform(); }
        }

        public void SetWaveform(PeakInfo[] peaks, double duration)
        {
            Duration = duration;
            var message = new PeaksReceivedEvent(Source, 0, Duration, peaks);
            HandlePeaks(message);
        }
    }
}