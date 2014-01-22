// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Query.Expressions
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
