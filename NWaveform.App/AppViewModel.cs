using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace NWaveform.App
{
    public class AppViewModel : Conductor<IPlayerViewModel>.Collection.OneActive
    {
        public AppViewModel(IEnumerable<IPlayerViewModel> players)
        {
            var viewModels = players as IPlayerViewModel[] ?? players.ToArray();
            Items.AddRange(viewModels);
            ActivateItem(viewModels[0]);
        }
    }
}