// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Annotations;
using Microsoft.OData.Edm.Values;

namespace System.Web.OData.Formatter.Serialization
{
    internal static class EdmDirectValueAnnotationsManagerExtensions
    {
        public static void SetActionLinkBuilder(this IEdmDirectValueAnnotationsManager manager, IEdmElement element,
            ActionLinkBuilder value)
        {
            SetCoreAnnotation<ActionLinkBuilder>(manager, element, value);
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