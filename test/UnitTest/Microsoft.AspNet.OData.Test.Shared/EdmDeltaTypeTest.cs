//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaTypeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class EdmDeltaTypeTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EntityType()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaType((IEdmEntityType)null, EdmDeltaEntityKind.Entry), "entityType");
        }

        [Fact]
        public void GetEdmType_Returns_EdmTypeInitializedByCtor()
        {
            IEdmEntityType _entityType = new EdmEntityType("NS", "Entity");
            var edmObject = new EdmDeltaType(_entityType, EdmDeltaEntityKind.Entry);

            Assert.Same(_entityType, edmObject.EntityType);
        }
    }
}
