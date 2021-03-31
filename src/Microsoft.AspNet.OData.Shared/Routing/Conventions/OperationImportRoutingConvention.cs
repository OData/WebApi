// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles function invocations.
    /// </summary>
    public partial class OperationImportRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext,
            IWebApiActionMap actionMap)
        {
            OperationImportSegment operationImportSegment = null;

            if (odataPath.PathTemplate == "~/unboundfunction" &&
                ODataRequestMethod.Get == controllerContext.Request.Method)
            {
                // The same function name may be used multiple times within a schema, each with a different set of parameters.
                // For unbound overloads the combination of the function name and the unordered set of parameter names
                // MUST identify a particular function overload.
                operationImportSegment = (OperationImportSegment)odataPath.Segments[0];
            }
            else if (odataPath.PathTemplate == "~/unboundaction" &&
                ODataRequestMethod.Post == controllerContext.Request.Method)
            {
                // The same action name may be used multiple times within a schema provided there is at most one unbound overload
                operationImportSegment = (OperationImportSegment)odataPath.Segments[0];
            }

            string actionName = SelectAction(operationImportSegment, actionMap);
            if (actionName != null)
            {
                controllerContext.AddFunctionParameterToRouteData(operationImportSegment);
                return actionName;
            }

            return null;
        }

        private static string SelectAction(OperationImportSegment operationImportSegment, IWebApiActionMap actionMap)
        {
            if (operationImportSegment == null)
            {
                return null;
            }

            IEdmOperationImport operationImport = operationImportSegment.OperationImports.FirstOrDefault();
            if (operationImport == null)
            {
                return null;
            }

            return actionMap.FindMatchingAction(operationImport.Name, "Invoke" + operationImport.Name);
        }
    }
}
