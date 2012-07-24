// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public class ArrayModelBinder<TElement> : CollectionModelBinder<TElement>
    {
        protected override bool CreateOrReplaceCollection(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext, IList<TElement> newCollection)
        {
            bindingContext.Model = newCollection.ToArray();
            return true;
        }
    }
}
