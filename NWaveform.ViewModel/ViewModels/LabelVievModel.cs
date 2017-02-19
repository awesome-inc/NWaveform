using System;
using System.Windows;
using System.Windows.Controls;
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
        private int _fontSize;
        private FontWeight _fontWeight;

        public string Text
        {
            get { return _text; }
            set { _text = value; NotifyOfPropertyChange(); }
        }

        public string Tooltip
        {
            get { return _tooltip; }
            set { _tooltip = value; NotifyOfPropertyChange(); }
        }

        public double Position
        {
            get { return _position; }
            set { _position = value; NotifyOfPropertyChange(); }
        }

        public double Magnitude
        {
            get { return _magnitude; }
            set { _magnitude = value; NotifyOfPropertyChange(); }
        }

        public IconChar Icon
        {
            get { return _icon; }
            set { _icon = value; NotifyOfPropertyChange(); }
        }

        public Brush Background
        {
            get { return _background; }
            set { _background = value; NotifyOfPropertyChange(); }
        }

        public Brush Foreground
        {
            get { return _foreground; }
            set { _foreground = value; NotifyOfPropertyChange(); }
        }

        public IMenuViewModel Menu
        {
            get { return _menu; }
            set { _menu = value; NotifyOfPropertyChange(); }
        }

        public IAudioSelectionViewModel Selection
        {
            get { return _selection; }
            set { _selection = value; NotifyOfPropertyChange(); }
        }

        public int FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; NotifyOfPropertyChange(); }
        }

        public FontWeight FontWeight
        {
            get { return _fontWeight; }
            set { _fontWeight = value; NotifyOfPropertyChange(); }
        }

        // ReSharper disable once UnusedMember.Global
        public void SuppressEmptyContextMenu(ContextMenuEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (Menu.IsEmpty()) e.Handled = true;
        }
    }
}