using System;
using System.Linq;
using Caliburn.Micro;

namespace NWaveform.App
{
    public class AppViewModel : Conductor<IPlayerViewModel>.Collection.OneActive
        , IHandle<CropAudioResponse>
    {
        private readonly IScopedFactory<IPlayerViewModel> _playerFactory;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public IChannelsViewModel Channels { get; }

        public AppViewModel(IEventAggregator events, IScopedFactory<IPlayerViewModel> playerFactory, IChannelsViewModel channels)
        {
            _playerFactory = playerFactory;
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (playerFactory == null) throw new ArgumentNullException(nameof(playerFactory));
            if (channels == null) throw new ArgumentNullException(nameof(channels));

            var viewModels = Enumerable.Range(0, 4).Select(i =>
            {
                var player = _playerFactory.Resolve();
                player.DisplayName = $"Player {i}";
                return player;
            }).ToList();

            Items.AddRange(viewModels);
            // ReSharper disable once VirtualMemberCallInConstructor
            ActivateItem(viewModels.First());

            Channels = channels;

            events.Subscribe(this);
        }

        public void AddPlayer()
        {
            var player = _playerFactory.Resolve();
            Items.Add(player);
            ActivateItem(player);
        }

        public override void DeactivateItem(IPlayerViewModel item, bool close)
        {
            base.DeactivateItem(item, close);
            if (close)
                _playerFactory.Release(item);
        }

        public void Handle(CropAudioResponse message)
        {
            var player = _playerFactory.Resolve();
            player.Source = message.CroppedAudioUri;
            Items.Add(player);
            ActivateItem(player);
        }
    }
}