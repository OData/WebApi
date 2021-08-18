//-----------------------------------------------------------------------------
// <copyright file="EdmModelBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace AspNetODataSample.Web.Models
{
    public static class EdmModelBuilder
    {
        private static IEdmModel _model = null;

        public static IEdmModel GetEdmModel()
        {
            if (_model == null)
            {
                var builder = new ODataConventionModelBuilder();
                builder.EntitySet<TodoItem>("TodoItems");

                var function = builder.Function("RateByOrder");
                function.Parameter<int>("order");
                function.Returns<string>();
                _model = builder.GetEdmModel();
            }

            return _model;
        }
    }
}
