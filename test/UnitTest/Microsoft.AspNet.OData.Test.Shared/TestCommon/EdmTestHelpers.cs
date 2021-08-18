//-----------------------------------------------------------------------------
// <copyright file="EdmTestHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Formatter.Deserialization;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;

namespace Microsoft.AspNet.OData.Test.Common
{
    internal static class EdmTestHelpers
    {
        private static IEdmModel _model;

        public static IEdmEntityType GetEDMProductType()
        {
            return GetModel().FindType("ODataDemo.Product") as IEdmEntityType;
        }

        public static IEdmType GetEdmType(string name)
        {
            return GetModel().FindType(name);
        }

        public static IEdmModel GetModel()
        {
            if (_model == null)
            {
                IEdmModel edmModel;
                IEnumerable<EdmError> edmErrors;
                if (CsdlReader.TryParse(XmlReader.Create(new StringReader(GetEdmx())), out edmModel, out edmErrors))
                {
                    _model = edmModel;
                    _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindDeclaredType("ODataDemo.Product"), new ClrTypeAnnotation(typeof(ODataResourceDeserializerTests.Product)));
                    _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindDeclaredType("ODataDemo.Supplier"), new ClrTypeAnnotation(typeof(ODataResourceDeserializerTests.Supplier)));
                    _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindDeclaredType("ODataDemo.Address"), new ClrTypeAnnotation(typeof(ODataResourceDeserializerTests.Address)));
                    _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindDeclaredType("ODataDemo.Category"), new ClrTypeAnnotation(typeof(ODataResourceDeserializerTests.Category)));
                    return _model;
                }
                else
                {
                    throw new NotSupportedException(string.Format("Parsing csdl failed with errors {0}", String.Join("\n", edmErrors.Select((edmError => edmError.ErrorMessage)))));
                }
            }
            else
            {
                return _model;
            }
        }

        public static string GetEdmx()
        {
            return Resources.ProductsCsdl;
        }

        public static EdmStructuralProperty StructuralProperty<TObject, TProperty>(IEdmStructuredType declaringType, Expression<Func<TObject, TProperty>> property, IEdmTypeReference propertyType = null)
        {
            PropertyInfo pInfo = ((property.Body as MemberExpression).Member as PropertyInfo);
            if (pInfo == null)
            {
                throw new InvalidOperationException(String.Format("Test error: {0} is not a property access lambda", property));
            }

            return new EdmStructuralProperty(declaringType, pInfo.Name, propertyType ?? EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(pInfo.PropertyType));
        }

        public static IEdmEntityTypeReference AsReference(this IEdmEntityType entity)
        {
            return new EdmEntityTypeReference(entity, isNullable: false);
        }

        public static IEdmComplexTypeReference AsReference(this IEdmComplexType complex)
        {
            return new EdmComplexTypeReference(complex, isNullable: false);
        }
    }
}
