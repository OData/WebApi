// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;

#if ASPNETWEBAPI
using System.Web.Http.Properties;
using SubRouteType = System.Web.Http.Routing.IHttpRoute;
#else
using System.Web.Mvc.Properties;
using SubRouteType = System.Web.Routing.Route;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>
    /// This class is similar to HttpRouteCollection, but route name is optional.
    /// </summary>
    /// <remarks>
    /// This is used in attribute routing, where we want to match multiple routes, and then later
    /// disambiguate which one is best.
    /// </remarks>
#if ASPNETWEBAPI
    internal class HttpSubRouteCollection : ICollection<SubRouteType>
#else
    internal class SubRouteCollection : ICollection<SubRouteType>
#endif
    {
        private readonly List<SubRouteType> _collection = new List<SubRouteType>();
        private readonly IDictionary<string, SubRouteType> _dictionary = new Dictionary<string, SubRouteType>(StringComparer.OrdinalIgnoreCase);

        public void Add(string name, SubRouteType route)
        {
            Contract.Assert(route != null);

            _collection.Add(route);

            if (name != null)
            {
                if (_dictionary.ContainsKey(name))
                {
                    ThrowExceptionForDuplicateRouteNames(name, route, _dictionary[name]);
                }
                else
                {
                    _dictionary.Add(name, route);
                }
            }
        }

        public void Add(SubRouteType item)
        {
            Contract.Assert(item != null);
            _collection.Add(item);
        }

        public void Clear()
        {
            _collection.Clear();
            _dictionary.Clear();
        }

        public bool Contains(SubRouteType item)
        {
            Contract.Assert(item != null);
            return _collection.Contains(item);
        }

        public void CopyTo(SubRouteType[] array, int arrayIndex)
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

        public bool Remove(SubRouteType item)
        {
            Contract.Assert(item != null);

            if (_dictionary.Values.Contains(item))
            {
                _dictionary.Values.Remove(item);
            }

            return _collection.Remove(item);
        }

        public IEnumerator<SubRouteType> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_collection).GetEnumerator();
        }

        public SubRouteType this[int index]
        {
            get { return _collection[index]; }
        }

        public SubRouteType this[string name]
        {
            get { return _dictionary[name]; }
        }

        public IEnumerable<KeyValuePair<string, SubRouteType>> NamedRoutes
        {
            get { return _dictionary; }
        }

        private void ThrowExceptionForDuplicateRouteNames(string name, SubRouteType route1, SubRouteType route2)
        {
#if ASPNETWEBAPI
            throw new InvalidOperationException(String.Format(
                CultureInfo.CurrentCulture,
                SRResources.SubRouteCollection_DuplicateRouteName,
                name,
                route1.RouteTemplate,
                route2.RouteTemplate));
#else
            throw new InvalidOperationException(String.Format(
                CultureInfo.CurrentCulture,
                MvcResources.SubRouteCollection_DuplicateRouteName,
                name,
                route1.Url,
                route2.Url));
#endif
        }
    }
}
