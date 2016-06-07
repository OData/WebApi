// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
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
            Assert.ThrowsArgumentNull(() => new ODataPathSegmentTranslator(model: null, 
                parameterAliasNodes: null), "model");
        }
    }
}
