//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaCollectionTypeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class EdmDeltaCollectionTypeTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EntityTypeReference()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaCollectionType((IEdmTypeReference)null), "entityTypeReference");
        }


        [Fact]
        public void GetEdmType_Returns_EdmTypeInitializedByCtor()
        {
            IEdmEntityTypeReference _entityReferenceType = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), isNullable: false);
            var edmObject = new EdmDeltaCollectionType(_entityReferenceType);

            Assert.Same(_entityReferenceType, edmObject.ElementType);
        }
    }
}
