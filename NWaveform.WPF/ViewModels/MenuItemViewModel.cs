using System.Collections.Generic;
using System.Windows.Input;
using FontAwesome.Sharp;

namespace NWaveform.ViewModels
{
    public class MenuItemViewModel : MenuViewModel, IMenuItemViewModel
    {
        private string _header;
        private ICommand _command;
        private IconChar _icon;
        private string _description;

        public MenuItemViewModel(IEnumerable<IMenuItemViewModel> items = null)
            : base(items)
        {
        }

        public string Header
        {
            get => _header;
            set
            {
                if (_header == value) return;
                _header = value;
                NotifyOfPropertyChange();
            }
        }

        public ICommand Command
        {
            get => _command;
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
            if (Icon == IconChar.None) Icon = delegateComand.IconChar;
        }

        public IconChar Icon
        {
            get => _icon;
            set
            {
                if (_icon == value) return;
                _icon = value;
                NotifyOfPropertyChange();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description == value) return;
                _description = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
