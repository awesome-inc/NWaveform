using System;
using System.Linq;
using Autofac;
using Caliburn.Micro;

namespace NWaveform.App
{
    public class AppViewModel : Conductor<IPlayerViewModel>.Collection.OneActive
        , IHandle<CropAudioResponse>
    {
        public IChannelsViewModel Channels { get; }
        private readonly IComponentContext _container;

        public AppViewModel(IEventAggregator events, IComponentContext container, IChannelsViewModel channels)
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
            item.Source = null; // dispose!
            base.DeactivateItem(item, close);
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
            return _container.Resolve<IPlayerViewModel>();
        }
    }
}