// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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