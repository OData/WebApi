// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Web.Http.ValueProviders.Providers
{
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "It is more fundamentally a value provider than a collection")]
    public class CompositeValueProvider : Collection<IValueProvider>, IValueProvider, IEnumerableValueProvider
    {
        public CompositeValueProvider()
        {
        }

        public CompositeValueProvider(IList<IValueProvider> list)
            : base(list)
        {
        }

        public virtual bool ContainsPrefix(string prefix)
        {
            return this.Any(vp => vp.ContainsPrefix(prefix));
        }

        public virtual ValueProviderResult GetValue(string key)
        {
            return (from provider in this
                    let result = provider.GetValue(key)
                    where result != null
                    select result).FirstOrDefault();
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            return (from provider in this
                    let result = GetKeysFromPrefixFromProvider(provider, prefix)
                    where result != null && result.Any()
                    select result).FirstOrDefault() ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
                throw Error.ArgumentNull("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IValueProvider item)
        {
            if (item == null)
            {
                throw Error.ArgumentNull("item");
            }
            base.SetItem(index, item);
        }
    }
}
