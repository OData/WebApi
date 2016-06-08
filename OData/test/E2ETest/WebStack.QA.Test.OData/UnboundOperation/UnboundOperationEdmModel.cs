// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace WebStack.QA.Test.OData.UnboundOperation
{
    internal class UnboundFunctionEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ConventionCustomer>("ConventionCustomers");
            builder.EntitySet<ConventionOrder>("ConventionOrders");

            EnumTypeConfiguration<ConventionGender> enumType = builder.EnumType<ConventionGender>();
            enumType.Member(ConventionGender.Female);
            enumType.Member(ConventionGender.Male);
            #region functions

            FunctionConfiguration getAllCustomers = builder.Function("GetAllConventionCustomers");
            getAllCustomers.ReturnsCollectionFromEntitySet<ConventionCustomer>("ConventionCustomers");
            getAllCustomers.IsComposable = true;

            // Return all the customers whose name contains CustomerName
            FunctionConfiguration getAllCustomersOverload = builder.Function("GetAllConventionCustomers");
            getAllCustomersOverload.ReturnsCollectionFromEntitySet<ConventionCustomer>("ConventionCustomers");
            getAllCustomersOverload.Parameter<string>("CustomerName");
            getAllCustomersOverload.IsComposable = true;

            FunctionConfiguration getCustomersById = builder.Function("GetConventionCustomerById");
            getCustomersById.Parameter<int>("CustomerId");
            getCustomersById.ReturnsFromEntitySet<ConventionCustomer>("ConventionCustomers");
            getCustomersById.IsComposable = true;

            FunctionConfiguration getOrder = builder.Function("GetConventionOrderByCustomerIdAndOrderName");
            getOrder.Parameter<int>("CustomerId");
            getOrder.Parameter<string>("OrderName");
            getOrder.ReturnsFromEntitySet<ConventionOrder>("ConventionOrders");

            FunctionConfiguration getCustomerNameById = builder.Function("GetConventionCustomerNameById");
            getCustomerNameById.Parameter<int>("CustomerId");
            getCustomerNameById.Returns<string>();

            FunctionConfiguration getDefinedGenders = builder.Function("GetDefinedGenders");
            getDefinedGenders.ReturnsCollection<ConventionGender>()
                .IsComposable = true;

            FunctionConfiguration function = builder.Function("AdvancedFunction").Returns<bool>();
            function.CollectionParameter<int>("nums");
            function.CollectionParameter<ConventionGender>("genders");
            function.Parameter<ConventionAddress>("location");
            function.CollectionParameter<ConventionAddress>("addresses");
            function.EntityParameter<ConventionCustomer>("customer");
            function.CollectionEntityParameter<ConventionCustomer>("customers");

            #endregion

            #region actions

            ActionConfiguration resetDataSource = builder.Action("ResetDataSource");

            // bug: error message:  non-binding parameter type must be either Primitive, Complex, Collection of Primitive or a Collection of Complex.
            /*
            ActionConfiguration createCustomer = builder.Action("CreateCustomer");
            createCustomer.Parameter<ConventionCustomer>("Customer");
            createCustomer.ReturnsFromEntitySet<ConventionCustomer>("ConventionCustomers");
           */

            ActionConfiguration udpateAddress = builder.Action("UpdateAddress");
            udpateAddress.Parameter<ConventionAddress>("Address");
            udpateAddress.Parameter<int>("ID");
            udpateAddress.ReturnsCollectionFromEntitySet<ConventionCustomer>("ConventionCustomers");

            ActionConfiguration action = builder.Action("AdvancedAction");
            action.CollectionParameter<int>("nums");
            action.CollectionParameter<ConventionGender>("genders");
            action.Parameter<ConventionAddress>("location");
            action.CollectionParameter<ConventionAddress>("addresses");
            action.EntityParameter<ConventionCustomer>("customer");
            action.CollectionEntityParameter<ConventionCustomer>("customers");

            #endregion

            var schemaNamespace = typeof(ConventionCustomer).Namespace;

            builder.Namespace = schemaNamespace;

            var edmModel = builder.GetEdmModel();
            var container = edmModel.EntityContainer as EdmEntityContainer;

            #region function imports

            var entitySet = container.FindEntitySet("ConventionCustomers");
            var getCustomersByIdOfEdmFunction = edmModel.FindDeclaredOperations(schemaNamespace + ".GetConventionCustomerById").First() as EdmFunction;
            container.AddFunctionImport("GetConventionCustomerByIdImport", getCustomersByIdOfEdmFunction, new EdmEntitySetReferenceExpression(entitySet));

            var functionsOfGetAllConventionCustomers = edmModel.FindDeclaredOperations(schemaNamespace + ".GetAllConventionCustomers");
            var getAllConventionCustomersOfEdmFunction = functionsOfGetAllConventionCustomers.FirstOrDefault(f => f.Parameters.Count() == 0) as EdmFunction;
            container.AddFunctionImport("GetAllConventionCustomersImport", getAllConventionCustomersOfEdmFunction, new EdmEntitySetReferenceExpression(entitySet));

            // TODO delete this overload after bug 1640 is fixed: It can not find the correct overload function if the the function is exposed as a function import.
            var getAllConventionCustomersOverloadOfEdmFunction = functionsOfGetAllConventionCustomers.FirstOrDefault(f => f.Parameters.Count() > 0) as EdmFunction;
            container.AddFunctionImport("GetAllConventionCustomersImport", getAllConventionCustomersOverloadOfEdmFunction, new EdmEntitySetReferenceExpression(entitySet));

            var entitySet1 = container.FindEntitySet("ConventionOrders");
            var GetConventionOrderByCustomerIdAndOrderNameOfEdmFunction = edmModel.FindDeclaredOperations(schemaNamespace + ".GetConventionOrderByCustomerIdAndOrderName").First() as EdmFunction;
            container.AddFunctionImport("GetConventionOrderByCustomerIdAndOrderNameImport", GetConventionOrderByCustomerIdAndOrderNameOfEdmFunction, new EdmEntitySetReferenceExpression(entitySet1));

            var getConventionCustomerNameByIdOfEdmFunction = edmModel.FindDeclaredOperations(schemaNamespace + ".GetConventionCustomerNameById").First() as EdmFunction;
            container.AddFunctionImport("GetConventionCustomerNameByIdImport", getConventionCustomerNameByIdOfEdmFunction, null);

            #endregion

            #region action imports

            var resetDataSourceOfEdmAction = edmModel.FindDeclaredOperations(schemaNamespace + ".ResetDataSource").FirstOrDefault() as EdmAction;
            container.AddActionImport("ResetDataSourceImport", resetDataSourceOfEdmAction);

            // TODO: it is a potential issue that entity can not be used as an un-bound parameter.
            /*
            var createCustomerOfEdmAction = edmModel.FindDeclaredOperations(schemaNamespace + ".CreateCustomer").FirstOrDefault() as EdmAction;
            container.AddActionImport("CreateCustomerImport", createCustomerOfEdmAction);
            */
            var updateAddressOfEdmAction = edmModel.FindDeclaredOperations(schemaNamespace + ".UpdateAddress").FirstOrDefault() as EdmAction;
            container.AddActionImport("UpdateAddressImport", updateAddressOfEdmAction, new EdmEntitySetReferenceExpression(entitySet));

            #endregion

            return edmModel;
        }
    }
}
