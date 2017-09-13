// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;

namespace WebStack.QA.Test.OData.Common.TypeCreator
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
