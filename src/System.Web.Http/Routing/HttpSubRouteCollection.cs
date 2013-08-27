// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Web.Http.Routing
{
    // Similar to HttpRouteCollection, but route name is optional.
    internal class HttpSubRouteCollection : ICollection<IHttpRoute>
    {
        private readonly List<IHttpRoute> _collection = new List<IHttpRoute>();
        private readonly IDictionary<string, IHttpRoute> _dictionary = new Dictionary<string, IHttpRoute>(StringComparer.OrdinalIgnoreCase);

        public void Add(string name, IHttpRoute route)
        {
            Contract.Assert(route != null);

            _collection.Add(route);

            if (name != null)
            {
                _dictionary.Add(name, route);
            }
        }

        public void Add(IHttpRoute item)
        {
            Contract.Assert(item != null);
            _collection.Add(item);
        }

        public void Clear()
        {
            _collection.Clear();
            _dictionary.Clear();
        }

        public bool Contains(IHttpRoute item)
        {
            Contract.Assert(item != null);
            return _collection.Contains(item);
        }

        public void CopyTo(IHttpRoute[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _collection.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IHttpRoute item)
        {
            Contract.Assert(item != null);

            if (_dictionary.Values.Contains(item))
            {
                _dictionary.Values.Remove(item);
            }

            return _collection.Remove(item);
        }

        public IEnumerator<IHttpRoute> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_collection).GetEnumerator();
        }

        public IHttpRoute this[int index]
        {
            get { return _collection[index]; }
        }

        public IHttpRoute this[string name]
        {
            get { return _dictionary[name]; }
        }

        public IEnumerable<KeyValuePair<string, IHttpRoute>> NamedRoutes
        {
            get { return _dictionary; }
        }
    }
}
