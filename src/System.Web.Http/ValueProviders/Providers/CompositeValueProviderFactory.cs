// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.Http.ValueProviders.Providers
{
    public class CompositeValueProviderFactory : ValueProviderFactory
    {
        private ValueProviderFactory[] _factories;

        public CompositeValueProviderFactory(IEnumerable<ValueProviderFactory> factories)
        {
            _factories = factories.ToArray();
        }

        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            return GetValueProvider(actionContext, _factories);
        }

        // Get a single ValueProvider from a collection of factories. 
        // This will never return null.
        internal static IValueProvider GetValueProvider(HttpActionContext actionContext, ValueProviderFactory[] factories)
        {
            // Fast-path the case of just one 
            if (factories.Length == 1)
            {
                IValueProvider provider = factories[0].GetValueProvider(actionContext);
                if (provider != null)
                {
                    return provider;
                }
            }

            List<IValueProvider> providers = factories.Select((f) => f.GetValueProvider(actionContext)).Where((vp) => vp != null).ToList();
            return new CompositeValueProvider(providers);
        }
    }
}
