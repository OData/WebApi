using System.Collections.Generic;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;

namespace WebStack.QA.Test.OData.UriParserExtension
{
    public class UriParserExtenstionEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");

            builder.EntityType<Customer>().Function("CalculateSalary").Returns<int>().Parameter<int>("month");
            builder.EntityType<Customer>().Action("UpdateAddress");
            builder.EntityType<Customer>()
                .Collection.Function("GetCustomerByGender")
                .ReturnsCollectionFromEntitySet<Customer>("Customers")
                .Parameter<Gender>("gender");

            return builder.GetEdmModel();
        }

        public static IEdmModel GetEdmModelWithAlternateKeys()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");

            EdmModel edmModel = new EdmModel();
            edmModel.AddElements(builder.GetEdmModel().SchemaElements);
            IEdmEntityType customerType = (IEdmEntityType)edmModel.FindDeclaredType("WebStack.QA.Test.OData.UriParserExtension.Customer"); 
            IEdmProperty nameProperty = customerType.FindProperty("Name");

            edmModel.AddAlternateKeyAnnotation(customerType, new Dictionary<string, IEdmProperty>() { { "Name", nameProperty } });

            return edmModel;
        }
    }
}
