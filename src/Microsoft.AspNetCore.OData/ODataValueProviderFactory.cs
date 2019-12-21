// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
#if NETSTANDARD2_0
    using Microsoft.AspNetCore.Mvc.Internal;
#endif
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNet.OData.Routing
{
    internal class ODataValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            ODataValueProvider valueProvider = new ODataValueProvider(
                context.ActionContext.HttpContext.Request.ODataFeature().RoutingConventionsStore);

            context.ValueProviders.Add(valueProvider);

            return Task.CompletedTask;
        }

        private class ODataValueProvider : BindingSourceValueProvider
        {
            private IDictionary<string, object> values;
            private PrefixContainer _prefixContainer;

            public ODataValueProvider(IDictionary<string, object> values)
                : base(BindingSource.Path)
            {
                this.values = values;
            }

            /// <inheritdoc />
            public override  bool ContainsPrefix(string key)
            {
                if (_prefixContainer == null)
                {
                    _prefixContainer = new PrefixContainer(values.Keys);
                }

                return _prefixContainer.ContainsPrefix(key);
            }

            /// <inheritdoc />
            public override ValueProviderResult GetValue(string key)
            {
                if (key == null)
                {
                    throw Error.ArgumentNull("key");
                }

                object value;
                if (values.TryGetValue(key, out value))
                {
                    var stringValue = value as string ?? value?.ToString() ?? string.Empty;
                    return new ValueProviderResult(stringValue);
                }
                else
                {
                    return ValueProviderResult.None;
                }
            }
        }
    }
}
