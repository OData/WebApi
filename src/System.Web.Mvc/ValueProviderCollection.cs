// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Web.Mvc
{
    public class ValueProviderCollection : Collection<IValueProvider>, IValueProvider, IUnvalidatedValueProvider, IEnumerableValueProvider
    {
        public ValueProviderCollection()
        {
        }

        public ValueProviderCollection(IList<IValueProvider> list)
            : base(list)
        {
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            return this.Any(vp => vp.ContainsPrefix(prefix));
        }

        public virtual ValueProviderResult GetValue(string key)
        {
            return GetValue(key, skipValidation: false);
        }

        public virtual ValueProviderResult GetValue(string key, bool skipValidation)
        {
            return (from provider in this
                    let result = GetValueFromProvider(provider, key, skipValidation)
                    where result != null
                    select result).FirstOrDefault();
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            return (from provider in this
                    let result = GetKeysFromPrefixFromProvider(provider, prefix)
                    where result != null && result.Any()
                    select result).FirstOrDefault() ?? new Dictionary<string, string>();
        }

        internal static ValueProviderResult GetValueFromProvider(IValueProvider provider, string key, bool skipValidation)
        {
            // Since IUnvalidatedValueProvider is a superset of IValueProvider, it's always OK to use the
            // IUnvalidatedValueProvider-supplied members if they're present. Otherwise just call the
            // normal IValueProvider members.

            IUnvalidatedValueProvider unvalidatedProvider = provider as IUnvalidatedValueProvider;
            return (unvalidatedProvider != null) ? unvalidatedProvider.GetValue(key, skipValidation) : provider.GetValue(key);
        }

        internal static IDictionary<string, string> GetKeysFromPrefixFromProvider(IValueProvider provider, string prefix)
        {
            IEnumerableValueProvider enumeratedProvider = provider as IEnumerableValueProvider;
            return (enumeratedProvider != null) ? enumeratedProvider.GetKeysFromPrefix(prefix) : null;
        }

        protected override void InsertItem(int index, IValueProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IValueProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }
    }
}
