// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Routing
{
    internal class ODataParameterValue
    {
        // This prefix is used to identify parameters in [FromODataUri] binding scenario.
        public const string ParameterValuePrefix = "DF908045-6922-46A0-82F2-2F6E7F43D1B1_";

        public ODataParameterValue(object paramValue, IEdmTypeReference paramType)
        {
            if (paramType == null)
            {
                throw Error.ArgumentNull("paramType");
            }

            Value = paramValue;
            EdmType = paramType;
        }

        public IEdmTypeReference EdmType { get; private set; }

        public object Value { get; private set; }
    }
}
