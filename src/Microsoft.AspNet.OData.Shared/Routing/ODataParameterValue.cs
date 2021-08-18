//-----------------------------------------------------------------------------
// <copyright file="ODataParameterValue.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Routing
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
