//-----------------------------------------------------------------------------
// <copyright file="ODataModelBinderConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class ODataModelBinderConverterTests
    {
        /// <summary>
        /// The set of potential values to test against 
        /// <see cref="ODataModelBinderConverter.Convert(object, IEdmTypeReference, Type, string, OData.Formatter.Deserialization.ODataDeserializerContext, IServiceProvider)"/>.
        /// </summary>
        public static TheoryDataSet<object, EdmPrimitiveTypeKind, Type, object> ODataModelBinderConverter_Works_TestData
        {
            get
            {
                return new TheoryDataSet<object, EdmPrimitiveTypeKind, Type, object>
                {
                    { "true", EdmPrimitiveTypeKind.Boolean, typeof(bool), true },
                    { true, EdmPrimitiveTypeKind.Boolean, typeof(bool), true },
                    { 5, EdmPrimitiveTypeKind.Int32, typeof(int),  5 },
                    { new Guid("C2AEFDF2-B533-4971-8B6A-A539373BFC32"), EdmPrimitiveTypeKind.Guid, typeof(Guid), new Guid("C2AEFDF2-B533-4971-8B6A-A539373BFC32") }
                };
            }
        }

        /// <summary>
        /// Tests the <see cref="ODataModelBinderConverter.Convert(object, IEdmTypeReference, Type, string, OData.Formatter.Deserialization.ODataDeserializerContext, IServiceProvider)"/>
        /// method to ensure proper operation against primitive types.
        /// </summary>
        /// <param name="odataValue">The value as it would come across the wire.</param>
        /// <param name="edmTypeKind">The <see cref="EdmPrimitiveTypeKind"/> to test for.</param>
        /// <param name="clrType">The CLR type to convert to.</param>
        /// <param name="expectedResult">The expected value in the correct type to check against.</param>
        /// <remarks>Contributed by Robert McLaws (@robertmclaws).</remarks>
        [Theory]
        [MemberData(nameof(ODataModelBinderConverter_Works_TestData))]
        public void Convert_CheckPrimitives(object odataValue, EdmPrimitiveTypeKind edmTypeKind, Type clrType, object expectedResult)
        {
            var edmTypeReference = new EdmPrimitiveTypeReference(EdmCoreModel.Instance.GetPrimitiveType(edmTypeKind), false);
            var value = new ConstantNode(odataValue, odataValue.ToString(), edmTypeReference);
            var result = ODataModelBinderConverter.Convert(value, edmTypeReference, clrType, "IsActive", null, null);
            Assert.NotNull(result);
            Assert.IsType(clrType, result);
            Assert.Equal(expectedResult, result);
        }
    }
}
