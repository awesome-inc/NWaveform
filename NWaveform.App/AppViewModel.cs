using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;

namespace NWaveform.App
{
    public class AppViewModel : Conductor<IPlayerViewModel>.Collection.OneActive
        , IHandle<CropAudioResponse>
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public IChannelsViewModel Channels { get; }

        private readonly ILifetimeScope _container;
        private readonly Dictionary<IPlayerViewModel, ILifetimeScope> _playerScopes = new Dictionary<IPlayerViewModel, ILifetimeScope>();

        public AppViewModel(IEventAggregator events, ILifetimeScope container, IChannelsViewModel channels)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (channels == null) throw new ArgumentNullException(nameof(channels));
            _container = container;

            var viewModels = Enumerable.Range(0, 4).Select(i =>
            {
                var player = CreatePlayer();
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
            var player = CreatePlayer();
            Items.Add(player);
            ActivateItem(player);
        }

        public override void DeactivateItem(IPlayerViewModel item, bool close)
        {
            base.DeactivateItem(item, close);

            if (close)
            {
                ILifetimeScope scope;
                if (_playerScopes.TryGetValue(item, out scope))
                {
                    _playerScopes.Remove(item);
                    scope.Dispose();
                }
            }
        }

        public void Handle(CropAudioResponse message)
        {
            var player = CreatePlayer();
            player.Source = message.CroppedAudioUri;
            Items.Add(player);
            ActivateItem(player);
        }

        private IPlayerViewModel CreatePlayer()
        {
            var scope = _container.BeginLifetimeScope();
            var player = scope.Resolve<IPlayerViewModel>();
            _playerScopes.Add(player, scope);
            return player;
        }
    }
}