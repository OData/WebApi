// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Routing
{
    public class ODataPathSegmentTranslatorTest
    {
        private readonly IEdmModel _model;
        private ODataPathSegmentTranslator _translator;

        public ODataPathSegmentTranslatorTest()
        {
            _model = ODataRoutingModel.GetModel();
            _translator = new ODataPathSegmentTranslator(_model, new Dictionary<string, SingleValueNode>());
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_IfMissModel()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new ODataPathSegmentTranslator(model: null, 
                parameterAliasNodes: null), "model");
        }
    }
}
