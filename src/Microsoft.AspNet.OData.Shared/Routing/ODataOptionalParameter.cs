//-----------------------------------------------------------------------------
// <copyright file="ODataOptionalParameter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Routing
{
    internal class ODataOptionalParameter
    {
        private List<IEdmOptionalParameter> _optionalParameters = new List<IEdmOptionalParameter>();

        public IReadOnlyList<IEdmOptionalParameter> OptionalParameters
        {
            get
            {
                return _optionalParameters;
            }
        }

        public void Add(IEdmOptionalParameter parameter)
        {
            _optionalParameters.Add(parameter);
        }
    }
}
