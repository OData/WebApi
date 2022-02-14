//-----------------------------------------------------------------------------
// <copyright file="ODataPathSegmentTranslatorTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing
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
