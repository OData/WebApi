// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
#if NETSTANDARD2_0
    using Microsoft.AspNetCore.Mvc.Internal;
#endif
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OData;

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
            private CultureInfo culture { get; }

            public ODataValueProvider(IDictionary<string, object> values, CultureInfo culture)
                : base(BindingSource.Path)
            {
                this.values = values;
                this.culture = culture;
            }

            public ODataValueProvider(IDictionary<string, object> values)
                : this(values, CultureInfo.InvariantCulture)
            {
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
                var prefixedKey = ODataParameterValue.ParameterValuePrefix + key;
                if (values.TryGetValue(key, out value))
                {
                    string stringValue = value as string ?? Convert.ToString(value, this.culture) ?? string.Empty;
                    return new ValueProviderResult(stringValue, this.culture);
                }
                else if (values.TryGetValue(prefixedKey, out value))
                {
                    var odataParameterValue = (ODataParameterValue)value;
                    if (odataParameterValue.Value is ODataEnumValue odataEnumValue)
                    {
                        return new ValueProviderResult(Convert.ToString(odataEnumValue.Value, this.culture) ?? string.Empty, this.culture);
                    }

                    string stringValue = odataParameterValue.Value as string ?? Convert.ToString(odataParameterValue.Value, this.culture) ?? string.Empty;
                    return new ValueProviderResult(stringValue, this.culture);
                }
                else
                {
                    return ValueProviderResult.None;
                }
            }
        }
    }
}
