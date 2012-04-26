// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class FromUriAttribute : ModelBinderAttribute
    {
        public override IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }

            foreach (ValueProviderFactory f in base.GetValueProviderFactories(controllerDescriptor))
            {
                if (f is IUriValueProviderFactory)
                {
                    yield return f;
                }
            }
        }
    }
}
