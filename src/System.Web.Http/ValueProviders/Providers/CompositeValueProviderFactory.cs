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
            List<IValueProvider> providers = _factories.Select<ValueProviderFactory, IValueProvider>((f) => f.GetValueProvider(actionContext)).Where((vp) => vp != null).ToList();
            return new CompositeValueProvider(providers);
        }
    }
}
