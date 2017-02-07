using System.Collections.Generic;
using Caliburn.Micro;
using NEdifis.Attributes;

namespace NWaveform.ViewModels
{
    [ExcludeFromConventions("DTO")]
    public class MenuViewModel : IMenuViewModel
    {
        public IEnumerable<IMenuItemViewModel> Items { get; }

        public MenuViewModel(IEnumerable<IMenuItemViewModel> items = null)
        {
            Items = items != null 
                ? new BindableCollection<IMenuItemViewModel>(items) 
                : new BindableCollection<IMenuItemViewModel>();
        }
    }
}