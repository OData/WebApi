// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IActionValueBinder"/>
    /// </summary>
    internal class ActionValueBinderTracer : IActionValueBinder
    {
        private readonly IActionValueBinder _innerBinder;
        private readonly ITraceWriter _traceWriter;

        public ActionValueBinderTracer(IActionValueBinder innerBinder, ITraceWriter traceWriter)
        {
            _innerBinder = innerBinder;
            _traceWriter = traceWriter;
        }

        // Creates wrapping tracers for all HttpParameterBindings
        HttpActionBinding IActionValueBinder.GetBinding(HttpActionDescriptor actionDescriptor)
        {
            HttpActionBinding actionBinding = _innerBinder.GetBinding(actionDescriptor);
            HttpParameterBinding[] parameterBindings = actionBinding.ParameterBindings;
            HttpParameterBinding[] newParameterBindings = new HttpParameterBinding[parameterBindings.Length];
            for (int i = 0; i < newParameterBindings.Length; i++)
            {
                HttpParameterBinding parameterBinding = parameterBindings[i];

                // Itercept FormatterParameterBinding to replace its formatters
                FormatterParameterBinding formatterParameterBinding = parameterBinding as FormatterParameterBinding;
                newParameterBindings[i] = formatterParameterBinding != null
                                            ? (HttpParameterBinding)new FormatterParameterBindingTracer(formatterParameterBinding, _traceWriter)
                                            : (HttpParameterBinding)new HttpParameterBindingTracer(parameterBinding, _traceWriter);
            }

            // Replace the inner HttpActionBinding's parameter bindings with our tracing versions.
            // This allows each individual parameter binding to trace.
            actionBinding.ParameterBindings = newParameterBindings;

            // Then create an HttpActionBindingTracer to wrap the actual HttpActionBinding
            return new HttpActionBindingTracer(actionBinding, _traceWriter);
        }
    }
}
