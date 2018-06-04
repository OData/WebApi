﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataSerializerTest
    {
        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            ODataSerializer serializer = new Mock<ODataSerializer>(ODataPayloadKind.Unsupported).Object;

            Assert.Equal(ODataPayloadKind.Unsupported, serializer.ODataPayloadKind);
        }

        [Fact]
        public void WriteObject_Throws_NotSupported()
        {
            ODataSerializer serializer = new Mock<ODataSerializer>(ODataPayloadKind.Unsupported) { CallBase = true }.Object;

            ExceptionAssert.Throws<NotSupportedException>(
                () => serializer.WriteObject(graph: null, type: typeof(int),messageWriter: null, writeContext: null),
                "ODataSerializerProxy does not support WriteObject.");
        }
    }
}
