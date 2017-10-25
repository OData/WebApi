﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Xml;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Nuwa;
using Xunit;

namespace WebStack.QA.Test.OData.ModelBuilder
{
    [NuwaFramework]
    public class ActionMetadataTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ActionProduct> products = builder.EntitySet<ActionProduct>("Products");
            ActionConfiguration productsByCategory = products.EntityType.Action("GetProductsByCategory");
            ActionConfiguration getSpecialProduct = products.EntityType.Action("GetSpecialProduct");
            productsByCategory.ReturnsCollectionFromEntitySet<ActionProduct>(products);
            getSpecialProduct.ReturnsFromEntitySet<ActionProduct>(products);
            return builder.GetEdmModel();
        }

        [Fact]
        public void ProvideOverloadToSupplyEntitySetConfiguration()
        {
            IEdmModel model = null;
            Stream stream = Client.GetStreamAsync(BaseAddress + "/odata/$metadata").Result;
            using (XmlReader reader = XmlReader.Create(stream))
            {
                model = CsdlReader.Parse(reader);
            }
            IEdmAction collection = model.FindDeclaredOperations("Default.GetProductsByCategory").SingleOrDefault() as IEdmAction;
            IEdmEntityType expectedReturnType = model.FindDeclaredType(typeof(ActionProduct).FullName) as IEdmEntityType;
            Assert.NotNull(expectedReturnType);
            Assert.NotNull(collection);
            Assert.True(collection.IsBound);
            Assert.NotNull(collection.ReturnType.AsCollection());
            Assert.NotNull(collection.ReturnType.AsCollection().ElementType().AsEntity());
            Assert.Equal(expectedReturnType, collection.ReturnType.AsCollection().ElementType().AsEntity().EntityDefinition());

            IEdmAction single = model.FindDeclaredOperations("Default.GetSpecialProduct").SingleOrDefault() as IEdmAction;
            Assert.NotNull(single);
            Assert.True(single.IsBound);
            Assert.NotNull(single.ReturnType.AsEntity());
            Assert.Equal(expectedReturnType, single.ReturnType.AsEntity().EntityDefinition());
        }
    }

    public class ActionProduct
    {
        public int Id { get; set; }
    }
}
