using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.Sharp;

namespace NWaveform.ViewModels
{
    public class LabelVievModel : ILabelVievModel
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
            set { _text = value; OnPropertyChanged(); }
        }

        public string Tooltip
        {
            get { return _tooltip; }
            set { _tooltip = value; OnPropertyChanged(); }
        }

        public double Position
        {
            get { return _position; }
            set { _position = value; OnPropertyChanged(); }
        }

        public double Magnitude
        {
            get { return _magnitude; }
            set { _magnitude = value; OnPropertyChanged(); }
        }

        public IconChar Icon
        {
            get { return _icon; }
            set { _icon = value; OnPropertyChanged(); }
        }

        public Brush Background
        {
            get { return _background; }
            set { _background = value; OnPropertyChanged(); }
        }

        public Brush Foreground
        {
            get { return _foreground; }
            set { _foreground = value; OnPropertyChanged(); }
        }

        public IMenuViewModel Menu
        {
            get { return _menu; }
            set { _menu = value; OnPropertyChanged(); }
        }

        public IAudioSelectionViewModel Selection
        {
            get { return _selection; }
            set { _selection = value; OnPropertyChanged(); }
        }

        public int FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; OnPropertyChanged(); }
        }

        public FontWeight FontWeight
        {
            get { return _fontWeight; }
            set { _fontWeight = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // ReSharper disable once UnusedMember.Global
        public void SuppressEmptyContextMenu(ContextMenuEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (Menu.IsEmpty()) e.Handled = true;
        }
    }
}