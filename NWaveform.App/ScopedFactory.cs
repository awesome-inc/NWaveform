using System;
using System.Collections.Generic;
using Autofac;

namespace NWaveform.App
{
    internal class ScopedFactory<T> : IScopedFactory<T>
    {
        private readonly ILifetimeScope _rootScope;
        private readonly Dictionary<T, ILifetimeScope> _itemScopes = new Dictionary<T, ILifetimeScope>();

        public ScopedFactory(ILifetimeScope rootScope)
        {
            _rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
        }

        public T Resolve()
        {
            var scope = _rootScope.BeginLifetimeScope();
            var item = scope.Resolve<T>();
            _itemScopes.Add(item, scope);
            return item;
        }

        public void Release(T item)
        {
            ILifetimeScope scope;
            if (_itemScopes.TryGetValue(item, out scope))
            {
                _itemScopes.Remove(item);
                scope.Dispose();
            }
        }
    }
}
