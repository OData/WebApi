// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class EdmDeltaTypeTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EntityType()
        {
            Assert.ThrowsArgumentNull(() => new EdmDeltaType((IEdmEntityType)null, EdmDeltaEntityKind.Entry), "entityType");
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
