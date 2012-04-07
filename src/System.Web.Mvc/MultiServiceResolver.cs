// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Mvc
{
    internal class MultiServiceResolver<TService> : IResolver<IEnumerable<TService>>
        where TService : class
    {
        private Lazy<IEnumerable<TService>> _itemsFromService;
        private Func<IEnumerable<TService>> _itemsThunk;
        private Func<IDependencyResolver> _resolverThunk;

        public MultiServiceResolver(Func<IEnumerable<TService>> itemsThunk)
        {
            if (itemsThunk == null)
            {
                throw new ArgumentNullException("itemsThunk");
            }

            _itemsThunk = itemsThunk;
            _resolverThunk = () => DependencyResolver.Current;
            _itemsFromService = new Lazy<IEnumerable<TService>>(() => _resolverThunk().GetServices<TService>());
        }

        internal MultiServiceResolver(Func<IEnumerable<TService>> itemsThunk, IDependencyResolver resolver)
            : this(itemsThunk)
        {
            if (resolver != null)
            {
                _resolverThunk = () => resolver;
            }
        }

        public IEnumerable<TService> Current
        {
            get { return _itemsFromService.Value.Concat(_itemsThunk()); }
        }
    }
}
