using Caliburn.Micro;

namespace NWaveform.App
{
    public class AppViewModel : Conductor<IPlayerViewModel>.Collection.OneActive
    {
        public AppViewModel(IPlayerViewModel player1, IPlayerViewModel player2)
        {
            Items.Add(player1);
            Items.Add(player2);
            ActivateItem(player1);
        }
    }
}