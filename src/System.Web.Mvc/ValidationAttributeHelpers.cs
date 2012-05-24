// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Security;

namespace System.Web.Mvc
{
    /// <summary>
    /// This class uses reflection to not take a dependency on .net 4.5
    /// </summary>
    internal static class ValidationAttributeHelpers
    {
        private static Assembly _systemWebAssembly = typeof(Membership).Assembly;
        private static Assembly _systemComponentModelDataAnnotationsAssembly = typeof(DataType).Assembly;

        public static readonly Type MembershipPasswordAttributeType = FindType(_systemWebAssembly, "System.Web.Security.MembershipPasswordAttribute");
        public static readonly Type CompareAttributeType = FindType(_systemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.CompareAttribute");

        public static readonly Type CreditCardAttributeType = FindType(_systemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.CreditCardAttribute");
        public static readonly Type EmailAddressAttributeType = FindType(_systemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.EmailAddressAttribute");
        public static readonly Type FileExtensionsAttributeType = FindType(_systemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.FileExtensionsAttribute");
        public static readonly Type PhoneAttributeType = FindType(_systemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.PhoneAttribute");
        public static readonly Type UrlAttributeType = FindType(_systemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.UrlAttribute");

        public static Func<ValidationAttribute, TProperty> GetPropertyDelegate<TProperty>(Type inputType, string propertyName)
        {
            if (inputType == null)
            {
                return null;
            }

            ParameterExpression attributeParameter = Expression.Parameter(typeof(ValidationAttribute));
            return Expression.Lambda<Func<ValidationAttribute, TProperty>>(
                Expression.Property(Expression.Convert(attributeParameter, inputType), propertyName),
                attributeParameter)
                .Compile();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We never want to fail here so we have to catch all exceptions.")]
        private static Type FindType(Assembly assembly, string typeName)
        {
            try
            {
                return assembly
                    .GetType(typeName, throwOnError: false);
            }
            catch
            {
                return null;
            }
        }
    }
}
