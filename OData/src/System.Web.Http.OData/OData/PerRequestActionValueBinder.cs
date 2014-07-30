// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.OData
{
    internal class PerRequestActionValueBinder : IActionValueBinder
    {
        private IActionValueBinder _innerActionValueBinder;

        public PerRequestActionValueBinder(IActionValueBinder innerActionValueBinder)
        {
            if (innerActionValueBinder == null)
            {
                throw Error.ArgumentNull("innerActionValueBinder");
            }

            _innerActionValueBinder = innerActionValueBinder;
        }

        public HttpActionBinding GetBinding(HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            HttpActionBinding binding = _innerActionValueBinder.GetBinding(actionDescriptor);

            if (binding == null)
            {
                return null;
            }

            HttpParameterBinding[] parameterBindings = binding.ParameterBindings;

            if (parameterBindings != null)
            {
                for (int i = 0; i < binding.ParameterBindings.Length; i++)
                {
                    HttpParameterBinding parameterBinding = binding.ParameterBindings[i];

                    // Replace the formatter parameter binding with one that will attach the request.
                    // Note that we do not replace any other types, including derived types, as we do not have a way to
                    // decorate/compose these instances; there is no way we can add request attachment behavior to an
                    // arbitrary implementation of HttpParameterBinding. Any custom parameter bindings that do not
                    // attach the request may fail when using with OData (and the exception retured in that instance
                    // will explain the necessity of providing this behavior when implementing HttpParameterBinding for
                    // OData).
                    if (parameterBinding != null && parameterBinding is FormatterParameterBinding)
                    {
                        Contract.Assert(parameterBinding.Descriptor != null);
                        Contract.Assert(actionDescriptor.Configuration != null);
                        Contract.Assert(actionDescriptor.Configuration.Formatters != null);
                        binding.ParameterBindings[i] = new PerRequestParameterBinding(parameterBinding.Descriptor,
                            actionDescriptor.Configuration.Formatters);
                    }
                }
            }

            return binding;
        }
    }
}
