// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation sources
    /// (entity sets or singletons)
    /// </summary>
    public abstract partial class NavigationSourceRoutingConvention
    {
        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller
        /// </returns>
        internal static SelectControllerResult SelectControllerImpl(ODataPath odataPath)
        {
            ODataPathSegment firstSegment = odataPath.Segments.FirstOrDefault();

            // entity set
            EntitySetSegment entitySetSegment = firstSegment as EntitySetSegment;
            if (entitySetSegment != null)
            {
                return new SelectControllerResult(entitySetSegment.EntitySet.Name, null);
            }

            // singleton
            SingletonSegment singletonSegment = firstSegment as SingletonSegment;
            if (singletonSegment != null)
            {
                return new SelectControllerResult(singletonSegment.Singleton.Name, null);
            }

            // operation import
            OperationImportSegment importSegment = firstSegment as OperationImportSegment;
            if (importSegment != null)
            {
                // There's two options: Each one has advantages/disadvantanges. Here picks #1.
                // 1) map all operation import to a certain controller, for example: ODataOperationImportController
                return new SelectControllerResult("ODataOperationImport", null);

                // 2) map operation import to controller named using operation improt name, for example:  ResetDataController
                // return new SelectControllerResult(importSegment.OperationImports.FirstOrDefault().Name, null);
            }

            return null;
        }
    }
}
