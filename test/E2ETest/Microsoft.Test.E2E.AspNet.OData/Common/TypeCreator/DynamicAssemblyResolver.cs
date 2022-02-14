//-----------------------------------------------------------------------------
// <copyright file="DynamicAssemblyResolver.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETFX // This class is only used in the AspNet version.
using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;

namespace Microsoft.Test.E2E.AspNet.OData.Common.TypeCreator
{
    public class DynamicHttpControllerTypeResolver : DefaultHttpControllerTypeResolver
    {
        private Func<ICollection<Type>, ICollection<Type>> resolver;

        public DynamicHttpControllerTypeResolver(Func<ICollection<Type>, ICollection<Type>> resolver)
            : base()
        {
            this.resolver = resolver;
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            var controllers = base.GetControllerTypes(assembliesResolver);
            controllers = this.resolver(controllers);
            return controllers;
        }
    }
}
#endif
