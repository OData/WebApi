// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNet.OData.Extensions;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// Implementation of <see cref="ModelBinderAttribute"/> used to bind an instance of <see cref="ODataPath"/> as an action parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed partial class ODataPathParameterBindingAttribute : ModelBinderAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathParameterBindingAttribute" /> class.
        /// </summary>
        public ODataPathParameterBindingAttribute()
        {
            this.BinderType = typeof(ODataPathParameterModelBinder);
        }

        /// <summary>
        /// Implementation of <see cref="IModelBinder"/> used to bind an instance of <see cref="ODataPath"/> as an action parameter.
        /// </summary>
        internal class ODataPathParameterModelBinder : IModelBinder
        {
            /// <inheritdoc />
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext == null)
                {
                    throw Error.ArgumentNull("bindingContext");
                }

                ODataPath odataPath = bindingContext.HttpContext.ODataFeature().Path;
                bindingContext.Result = ModelBindingResult.Success(odataPath);

                return TaskHelpers.Completed();
            }
        }
    }
}
