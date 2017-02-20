using System;
using System.Collections.Generic;
using System.Windows.Input;
using FontAwesome.Sharp;
using NEdifis.Attributes;

namespace NWaveform.ViewModels
{
    [ExcludeFromConventions("DTO")]
    public class MenuItemViewModel : MenuViewModel, IMenuItemViewModel
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
                NotifyOfPropertyChange();
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
                NotifyOfPropertyChange();
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
                NotifyOfPropertyChange();
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
                NotifyOfPropertyChange();
            }
        }

        public Uri HelpLink
        {
            get { return _helpLink; }
            set
            {
                if (Equals(_helpLink, value)) return;
                _helpLink = value;
                NotifyOfPropertyChange();
            }
        }
    }
}