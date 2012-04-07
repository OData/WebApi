// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace System.Web.Http.ApiExplorer
{
    public class AttributeDocumentationProvider : IDocumentationProvider
    {
        public string GetDocumentation(HttpActionDescriptor actionDescriptor)
        {
            var apiDocumentation = actionDescriptor.GetCustomAttributes<ApiDocumentationAttribute>().FirstOrDefault();
            if (apiDocumentation != null)
            {
                return apiDocumentation.Description;
            }

            return string.Empty;
        }

        public string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            var parameterDocumentation = parameterDescriptor.ActionDescriptor.GetCustomAttributes<ApiParameterDocumentationAttribute>().FirstOrDefault(param => param.ParameterName == parameterDescriptor.ParameterName);
            if (parameterDocumentation != null)
            {
                return parameterDocumentation.Description;
            }

            return string.Empty;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ApiDocumentationAttribute : Attribute
    {
        public ApiDocumentationAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ApiParameterDocumentationAttribute : Attribute
    {
        public ApiParameterDocumentationAttribute(string parameterName, string description)
        {
            ParameterName = parameterName;
            Description = description;
        }

        public string ParameterName { get; private set; }

        public string Description { get; private set; }
    }
}
