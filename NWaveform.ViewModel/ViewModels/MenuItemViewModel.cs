using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FontAwesome.Sharp;
using NEdifis.Attributes;

namespace NWaveform.ViewModels
{
    [ExcludeFromConventions("DTO")]
    public class MenuItemViewModel : MenuViewModel, IMenuItemViewModel, INotifyPropertyChanged
    {
        private string _header;
        private ICommand _command;
        private IconChar _icon;
        private string _description;
        private Uri _helpLink;

        public MenuItemViewModel(IEnumerable<IMenuItemViewModel> items = null)
            : base(items)
        {
        }

        public string Header
        {
            get { return _header; }
            set
            {
                if (_header == value) return;
                _header = value;
                OnPropertyChanged();
            }
        }

        public ICommand Command
        {
            get { return _command; }
            set
            {
                if (_command == value) return;
                _command = value;
                GuessCommandProperties();
                OnPropertyChanged();
            }
        }

        private void GuessCommandProperties()
        {
            var delegateComand = _command as DelegateCommand;
            if (delegateComand == null) return;
            if (string.IsNullOrWhiteSpace(Header)) Header = delegateComand.Title;
            if (string.IsNullOrWhiteSpace(Description)) Description = delegateComand.Description;
        }

        public IconChar Icon
        {
            get { return _icon; }
            set
            {
                if (_icon == value) return;
                _icon = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
                OnPropertyChanged();
            }
        }

        public Uri HelpLink
        {
            get { return _helpLink; }
            set
            {
                if (Equals(_helpLink, value)) return;
                _helpLink = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}