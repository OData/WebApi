// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

#if ASPNETWEBAPI
using System.Web.Http.Properties;
using SubRouteEntryType = System.Web.Http.Routing.HttpRouteEntry;
using SubRouteType = System.Web.Http.Routing.IHttpRoute;
#else
using System.Web.Mvc.Properties;
using SubRouteEntryType = System.Web.Mvc.Routing.RouteEntry;
using SubRouteType = System.Web.Routing.Route;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>Represents a collection of route entries and the routes they contain.</summary>
    /// <remarks>
    /// This is used in attribute routing, where we want to match multiple routes, and then later
    /// disambiguate which one is best.
    /// </remarks>
#if ASPNETWEBAPI
    internal class HttpSubRouteCollection : IReadOnlyCollection<SubRouteType>
#else
    internal class SubRouteCollection : IReadOnlyCollection<SubRouteType>
#endif
    {
        private readonly List<SubRouteType> _routes = new List<SubRouteType>();
        private readonly List<SubRouteEntryType> _entries = new List<SubRouteEntryType>();

        public void Add(SubRouteEntryType entry)
        {
            Contract.Assert(entry != null);
            SubRouteType route = entry.Route;
            Contract.Assert(route != null);

            string name = entry.Name;

            if (name != null)
            {
                SubRouteEntryType duplicateEntry = _entries.SingleOrDefault((e) => e.Name == name);

                if (duplicateEntry != null)
                {
                    ThrowExceptionForDuplicateRouteNames(name, route, duplicateEntry.Route);
                }
            }

            _routes.Add(route);
            _entries.Add(entry);
        }

        public int Count
        {
            get { return _entries.Count; }
        }

        public IEnumerator<SubRouteType> GetEnumerator()
        {
            return _routes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_routes).GetEnumerator();
        }

        public IReadOnlyCollection<SubRouteEntryType> Entries
        {
            get { return _entries; }
        }

        private static void ThrowExceptionForDuplicateRouteNames(string name, SubRouteType route1, SubRouteType route2)
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
