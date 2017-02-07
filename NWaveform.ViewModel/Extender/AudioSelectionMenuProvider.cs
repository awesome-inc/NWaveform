using System.Collections.Generic;
using System.Linq;
using NWaveform.ViewModels;

namespace NWaveform.Extender
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AudioSelectionMenuProvider : IAudioSelectionMenuProvider
    {
        private readonly IEnumerable<IAudioSelectionMenuExtender> _allMenuItems;
        private MenuViewModel _menu;

        public AudioSelectionMenuProvider(IEnumerable<IAudioSelectionMenuExtender> allMenuItems = null)
        {
            _allMenuItems = allMenuItems ?? Enumerable.Empty<IAudioSelectionMenuExtender>();
        }

        public IMenuViewModel Menu
        {
            get
            {
                if (_menu == null)
                {
                    var items = _allMenuItems.SelectMany(provider => provider.MenuItems);
                    _menu = new MenuViewModel(items);
                }
                return _menu;
            }
        }
    }
}