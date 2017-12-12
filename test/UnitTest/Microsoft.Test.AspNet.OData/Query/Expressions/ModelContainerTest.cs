// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Query.Expressions
{
    public class ModelContainerTest
    {
        [Fact]
        public void GetModelID_Returns_DifferentIDForDifferentModels()
        {
            EdmModel model1 = new EdmModel();
            EdmModel model2 = new EdmModel();

            Assert.NotEqual(ModelContainer.GetModelID(model1), ModelContainer.GetModelID(model2));
        }

        [Fact]
        public void GetModelID_Returns_SameIDForSameModel()
        {
            EdmModel model = new EdmModel();

            Assert.Equal(ModelContainer.GetModelID(model), ModelContainer.GetModelID(model));
        }

        [Fact]
        public void GetModelID_AndThen_GetModel_ReturnsOriginalModel()
        {
            EdmModel model = new EdmModel();
            string modelID = ModelContainer.GetModelID(model);

            Assert.Same(model, ModelContainer.GetModel(modelID));
        }
    }
}
