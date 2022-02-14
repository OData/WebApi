//-----------------------------------------------------------------------------
// <copyright file="EdmDirectValueAnnotationsManagerExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    internal static class EdmDirectValueAnnotationsManagerExtensions
    {
        public static void SetOperationLinkBuilder(this IEdmDirectValueAnnotationsManager manager, IEdmElement element,
            OperationLinkBuilder value)
        {
            SetCoreAnnotation<OperationLinkBuilder>(manager, element, value);
        }

        public static void SetIsAlwaysBindable(this IEdmDirectValueAnnotationsManager manager, IEdmOperation operation)
        {
            SetODataAnnotation(manager, operation, "IsAlwaysBindable", "true");
        }

        private static void SetCoreAnnotation<T>(this IEdmDirectValueAnnotationsManager manager, IEdmElement element,
            T value)
        {
            Contract.Assert(manager != null);
            manager.SetAnnotationValue(element, "http://schemas.microsoft.com/ado/2011/04/edm/internal",
                GetCoreAnnotationName(typeof(T)), value);
        }

        private static void SetODataAnnotation(this IEdmDirectValueAnnotationsManager manager, IEdmElement element,
            string name, string value)
        {
            Contract.Assert(manager != null);
            manager.SetAnnotationValue(element, "http://docs.oasis-open.org/odata/ns/metadata", name,
                new FakeEdmStringValue(value));
        }

        private static string GetCoreAnnotationName(Type t)
        {
            return t.FullName.Replace('.', '_');
        }

        private class FakeEdmStringValue : IEdmStringValue
        {
            private readonly string _value;

            public FakeEdmStringValue(string value)
            {
                _value = value;
            }

            public string Value
            {
                get { return _value; }
            }

            public IEdmTypeReference Type
            {
                get { throw new NotImplementedException(); }
            }

            public EdmValueKind ValueKind
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
