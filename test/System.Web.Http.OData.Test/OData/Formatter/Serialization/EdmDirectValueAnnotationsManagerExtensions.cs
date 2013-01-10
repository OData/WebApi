// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Values;

namespace System.Web.Http.OData.Formatter.Serialization
{
    internal static class EdmDirectValueAnnotationsManagerExtensions
    {
        public static void SetActionLinkBuilder(this IEdmDirectValueAnnotationsManager manager, IEdmElement element,
            ActionLinkBuilder value)
        {
            SetCoreAnnotation<ActionLinkBuilder>(manager, element, value);
        }

        public static void SetDefaultContainer(this IEdmDirectValueAnnotationsManager manager,
            IEdmEntityContainer container)
        {
            SetODataAnnotation(manager, container, "IsDefaultEntityContainer", "true");
        }

        public static void SetIsAlwaysBindable(this IEdmDirectValueAnnotationsManager manager,
            IEdmFunctionImport functionImport)
        {
            SetODataAnnotation(manager, functionImport, "IsAlwaysBindable", "true");
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
            manager.SetAnnotationValue(element, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", name,
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