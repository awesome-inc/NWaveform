using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Caliburn.Micro;
using FontAwesome.Sharp;

namespace NWaveform.ViewModels
{
    public class LabelVievModel : PropertyChangedBase, ILabelVievModel
    {
        private string _text;
        private double _position;
        private double _magnitude;
        private IconChar _icon;
        private Brush _background = new SolidColorBrush(Colors.MediumPurple) { Opacity = 0.5 };
        private Brush _foreground = SystemColors.ControlTextBrush;
        private string _tooltip;
        private IAudioSelectionViewModel _selection = new AudioSelectionViewModel();
        private IMenuViewModel _menu;
        private int _fontSize = Convert.ToInt32(TextElement.FontSizeProperty.DefaultMetadata.DefaultValue);
        private FontWeight _fontWeight;
        private double _iconRotation;
        private FlipOrientation _iconFlipOrientation;
        private bool _iconSpin;
        private double _iconSpinDuration = Convert.ToDouble(Awesome.SpinDurationProperty.DefaultMetadata.DefaultValue);

        public string Text
        {
            get => _text;
            set { _text = value; NotifyOfPropertyChange(); }
        }

        public string Tooltip
        {
            get => _tooltip;
            set { _tooltip = value; NotifyOfPropertyChange(); }
        }

        public double Position
        {
            get => _position;
            set { _position = value; NotifyOfPropertyChange(); }
        }

        public double Magnitude
        {
            get => _magnitude;
            set { _magnitude = value; NotifyOfPropertyChange(); }
        }

        public IconChar Icon
        {
            get => _icon;
            set { _icon = value; NotifyOfPropertyChange(); }
        }

        public Brush Background
        {
            get => _background;
            set { _background = value; NotifyOfPropertyChange(); }
        }

        public Brush Foreground
        {
            get => _foreground;
            set { _foreground = value; NotifyOfPropertyChange(); }
        }

        public IMenuViewModel Menu
        {
            get => _menu;
            set { _menu = value; NotifyOfPropertyChange(); }
        }

        public IAudioSelectionViewModel Selection
        {
            get => _selection;
            set { _selection = value; NotifyOfPropertyChange(); }
        }

        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                if (value.Equals(_fontSize)) return;
                NotifyOfPropertyChange();
            }
        }

        public FontWeight FontWeight
        {
            get => _fontWeight;
            set
            {
                if (value.Equals(_fontWeight)) return;
                _fontWeight = value;
                NotifyOfPropertyChange(); }
        }

        // ReSharper disable once UnusedMember.Global
        public void SuppressEmptyContextMenu(ContextMenuEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (Menu.IsEmpty()) e.Handled = true;
        }

        public double IconRotation
        {
            get => _iconRotation;
            set
            {
                if (value.Equals(_iconRotation)) return;
                _iconRotation = value;
                NotifyOfPropertyChange();
            }
        }

        public FlipOrientation IconFlipOrientation
        {
            get => _iconFlipOrientation;
            set
            {
                if (value == _iconFlipOrientation) return;
                _iconFlipOrientation = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IconSpin
        {
            get => _iconSpin;
            set
            {
                if (value == _iconSpin) return;
                _iconSpin = value;
                NotifyOfPropertyChange();
            }
        }

        public double IconSpinDuration
        {
            get => _iconSpinDuration;
            set
            {
                if (value.Equals(_iconSpinDuration)) return;
                _iconSpinDuration = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
