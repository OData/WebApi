// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Moq;

namespace System.Web.OData.TestCommon
{
    public static class ODataModelBuilderMocks
    {
        // Creates a mock of an ODataModelBuilder or any subclass of it that disables model validation
        // in order to reduce verbosity on tests.
        public static T GetModelBuilderMock<T>(params object[] parameters) where T : ODataModelBuilder
        {
            Mock<T> mock = new Mock<T>(parameters);
            mock.Setup(b => b.ValidateModel(It.IsAny<IEdmModel>())).Callback(() => { });
            mock.CallBase = true;
            return mock.Object;
        }
    }
}
