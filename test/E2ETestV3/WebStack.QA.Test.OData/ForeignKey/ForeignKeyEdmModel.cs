using System;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;

namespace WebStack.QA.Test.OData.ForeignKey
{
    public class ForeignKeyEdmModel
    {
        public static IEdmModel GetExplicitModel(bool foreignKey)
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var customer = builder.Entity<ForeignKeyCustomer>();
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Name);
            customer.HasMany(c => c.Orders);

            var order = builder.Entity<ForeignKeyOrder>();
            order.HasKey(o => o.OrderId);
            order.Property(o => o.OrderName);
            order.Property(o => o.CustomerId);

            EntitySetConfiguration<ForeignKeyCustomer> customers;
            EntitySetConfiguration<ForeignKeyOrder> orders;
            if (foreignKey)
            {
                order.HasRequired(o => o.Customer, (o, c) => o.CustomerId == c.Id).CascadeOnDelete(cascade: true);

                customers = builder.EntitySet<ForeignKeyCustomer>("ForeignKeyCustomers");
                orders = builder.EntitySet<ForeignKeyOrder>("ForeignKeyOrders");
                customers.HasManyBinding(c => c.Orders, "ForeignKeyOrders");
                orders.HasRequiredBinding(o => o.Customer, "ForeignKeyCustomers");
            }
            else
            {
                order.HasRequired(o => o.Customer);

                customers = builder.EntitySet<ForeignKeyCustomer>("ForeignKeyCustomersNoCascade");
                orders = builder.EntitySet<ForeignKeyOrder>("ForeignKeyOrdersNoCascade");
                customers.HasManyBinding(c => c.Orders, "ForeignKeyOrdersNoCascade");
                orders.HasRequiredBinding(o => o.Customer, "ForeignKeyCustomersNoCascade");
            }

            customers.HasIdLink(entityContext =>
            {
                object id;
                entityContext.EdmObject.TryGetPropertyValue("Id", out id);
                return entityContext.Url.Link("DefaultRouteName", new
                {
                    odataPath = entityContext.Url.CreateODataLink(
                        new EntitySetPathSegment(entityContext.EntitySet.Name),
                        new KeyValuePathSegment(id.ToString()))
                });
            }, true);

            orders.HasIdLink(entityContext =>
            {
                object id;
                entityContext.EdmObject.TryGetPropertyValue("OrderId", out id);
                return entityContext.Url.Link("DefaultRouteName", new
                {
                    odataPath = entityContext.Url.CreateODataLink(
                        new EntitySetPathSegment(entityContext.EntitySet.Name),
                        new KeyValuePathSegment(id.ToString()))
                });
            }, true);

            // Create navigation Link builders
            customers.HasNavigationPropertiesLink(
                customer.NavigationProperties,
                (entityContext, navigationProperty) =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("Id", out id);
                    return new Uri(entityContext.Url.Link("DefaultRouteName",
                new
                {
                    odataPath = entityContext.Url.CreateODataLink(
                        new EntitySetPathSegment(entityContext.EntitySet.Name),
                        new KeyValuePathSegment(id.ToString()),
                        new NavigationPathSegment(navigationProperty))
                }));
                }, true);

            orders.HasNavigationPropertiesLink(
                order.NavigationProperties,
                (entityContext, navigationProperty) =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("OrderId", out id);
                    return new Uri(entityContext.Url.Link("DefaultRouteName",
                new
                {
                    odataPath = entityContext.Url.CreateODataLink(
                        new EntitySetPathSegment(entityContext.EntitySet.Name),
                        new KeyValuePathSegment(id.ToString()),
                        new NavigationPathSegment(navigationProperty))
                }));
                }, true);

            BuildAction(builder);

            return builder.GetEdmModel();
        }

        public static IEdmModel GetConventionModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ForeignKeyCustomer>("ForeignKeyCustomers");
            builder.EntitySet<ForeignKeyOrder>("ForeignKeyOrders");

            BuildAction(builder);

            return builder.GetEdmModel();
        }

        private static void BuildAction(ODataModelBuilder builder)
        {
            builder.Entity<ForeignKeyCustomer>().Collection.Action("ResetDataSource");
        }
    }
}
