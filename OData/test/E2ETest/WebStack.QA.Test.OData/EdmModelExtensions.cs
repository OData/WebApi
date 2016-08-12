// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData
{
    public static class EdmModelExtensions
    {
        public static IEdmModel GetEdmModel(this HttpRequestMessage request)
        {
            return (IEdmModel)request.GetRequestContainer().GetService(typeof(IEdmModel));
        }
    }
}
