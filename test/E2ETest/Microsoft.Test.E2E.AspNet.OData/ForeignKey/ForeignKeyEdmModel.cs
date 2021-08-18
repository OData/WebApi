//-----------------------------------------------------------------------------
// <copyright file="ForeignKeyEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.ForeignKey
{
    public class ForeignKeyEdmModel
    {
        public static IEdmModel GetExplicitModel(bool foreignKey)
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var customer = builder.EntityType<ForeignKeyCustomer>();
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Name);
            customer.HasMany(c => c.Orders);

            var order = builder.EntityType<ForeignKeyOrder>();
            order.HasKey(o => o.OrderId);
            order.Property(o => o.OrderName);
            order.Property(o => o.CustomerId);

            EntitySetConfiguration<ForeignKeyCustomer> customers;
            EntitySetConfiguration<ForeignKeyOrder> orders;
            if (foreignKey)
            {
                order.HasOptional(o => o.Customer, (o, c) => o.CustomerId == c.Id).CascadeOnDelete(cascade: true);

                customers = builder.EntitySet<ForeignKeyCustomer>("ForeignKeyCustomers");
                orders = builder.EntitySet<ForeignKeyOrder>("ForeignKeyOrders");
                customers.HasManyBinding(c => c.Orders, "ForeignKeyOrders");
                orders.HasOptionalBinding(o => o.Customer, "ForeignKeyCustomers");
            }
            else
            {
                order.HasOptional(o => o.Customer);

                customers = builder.EntitySet<ForeignKeyCustomer>("ForeignKeyCustomersNoCascade");
                orders = builder.EntitySet<ForeignKeyOrder>("ForeignKeyOrdersNoCascade");
                customers.HasManyBinding(c => c.Orders, "ForeignKeyOrdersNoCascade");
                orders.HasOptionalBinding(o => o.Customer, "ForeignKeyCustomersNoCascade");
            }

            customers.HasIdLink(entityContext =>
            {
                object id;
                entityContext.EdmObject.TryGetPropertyValue("Id", out id);
                string uri = ResourceContextHelper.CreateODataLink(entityContext,
                    new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                    new KeySegment(new[] { new KeyValuePair<string, object>("Id", id)}, entityContext.StructuredType as IEdmEntityType, null));
                return new Uri(uri);
            }, true);

            orders.HasIdLink(entityContext =>
            {
                object id;
                entityContext.EdmObject.TryGetPropertyValue("OrderId", out id);
                string uri = ResourceContextHelper.CreateODataLink(entityContext,
                    new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                    new KeySegment(new[] { new KeyValuePair<string, object>("OrderId", id) }, entityContext.StructuredType as IEdmEntityType, null));
                return new Uri(uri);
            }, true);

            // Create navigation Link builders
            customers.HasNavigationPropertiesLink(
                customer.NavigationProperties,
                (entityContext, navigationProperty) =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("Id", out id);
                    string uri = ResourceContextHelper.CreateODataLink(entityContext,
                        new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                        new KeySegment(new[] { new KeyValuePair<string, object>("Id", id) }, entityContext.StructuredType as IEdmEntityType, null),
                        new NavigationPropertySegment(navigationProperty, null));
                    return new Uri(uri);
                }, true);

            orders.HasNavigationPropertiesLink(
                order.NavigationProperties,
                (entityContext, navigationProperty) =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("OrderId", out id);
                    string uri = ResourceContextHelper.CreateODataLink(entityContext,
                        new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                        new KeySegment(new[] { new KeyValuePair<string, object>("OrderId", id) }, entityContext.StructuredType as IEdmEntityType, null),
                        new NavigationPropertySegment(navigationProperty, null));
                    return new Uri(uri);
                }, true);

            BuildAction(builder);

            return builder.GetEdmModel();
        }

        public static IEdmModel GetConventionModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<ForeignKeyCustomer>("ForeignKeyCustomers");
            builder.EntitySet<ForeignKeyOrder>("ForeignKeyOrders");

            BuildAction(builder);

            return builder.GetEdmModel();
        }

        private static void BuildAction(ODataModelBuilder builder)
        {
            builder.Action("ResetDataSource");
            builder.Action("ResetDataSourceNonCacade");
        }
    }
}
