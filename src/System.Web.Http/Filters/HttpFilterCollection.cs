// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Web.Http.Filters
{
    public class HttpFilterCollection : IEnumerable<FilterInfo>
    {
        private readonly List<FilterInfo> _filters = new List<FilterInfo>();

        public int Count
        {
            get { return _filters.Count; }
        }

        public void Add(IFilter filter)
        {
            if (filter == null)
            {
                throw Error.ArgumentNull("filter");
            }

            AddInternal(new FilterInfo(filter, FilterScope.Global));
        }

        private void AddInternal(FilterInfo filter)
        {
            _filters.Add(filter);
        }

        public void Clear()
        {
            _filters.Clear();
        }

        public bool Contains(IFilter filter)
        {
            return _filters.Any(f => f.Instance == filter);
        }

        public IEnumerator<FilterInfo> GetEnumerator()
        {
            return _filters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Remove(IFilter filter)
        {
            _filters.RemoveAll(f => f.Instance == filter);
        }
    }
}
