using System;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;

namespace WebStack.QA.Test.OData.DateAndTimeOfDay
{
    public class DateAndTimeOfDayEdmModel
    {
        public static IEdmModel GetExplicitModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var customerType = builder.EntityType<DCustomer>().HasKey(c => c.Id);
            customerType.Property(c => c.DateTime);
            customerType.Property(c => c.Offset);
            customerType.Property(c => c.Date);
            customerType.Property(c => c.TimeOfDay);

            customerType.Property(c => c.NullableDateTime);
            customerType.Property(c => c.NullableOffset);
            customerType.Property(c => c.NullableDate);
            customerType.Property(c => c.NullableTimeOfDay);

            customerType.CollectionProperty(c => c.DateTimes);
            customerType.CollectionProperty(c => c.Offsets);
            customerType.CollectionProperty(c => c.Dates);
            customerType.CollectionProperty(c => c.TimeOfDays);

            customerType.CollectionProperty(c => c.NullableDateTimes);
            customerType.CollectionProperty(c => c.NullableOffsets);
            customerType.CollectionProperty(c => c.NullableDates);
            customerType.CollectionProperty(c => c.NullableTimeOfDays);

            var customers = builder.EntitySet<DCustomer>("DCustomers");
            customers.HasIdLink(link, true);
            customers.HasEditLink(link, true);

            BuildFunctions(builder);
            BuildActions(builder);

            return builder.GetEdmModel();
        }

        public static IEdmModel GetConventionModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DCustomer>("DCustomers");

            BuildFunctions(builder);
            BuildActions(builder);

            return builder.GetEdmModel();
        }

        private static void BuildFunctions(ODataModelBuilder builder)
        {
            FunctionConfiguration function = builder.EntityType<DCustomer>().Function("BoundFunction")
                    .ReturnsCollectionViaEntitySetPath<DCustomer>("bindingParameter");
            function.Parameter<Date>("modifiedDate");
            function.Parameter<TimeOfDay>("modifiedTime");
            function.Parameter<Date?>("nullableModifiedDate");
            function.Parameter<TimeOfDay?>("nullableModifiedTime");

            function = builder.Function("UnboundFunction").ReturnsCollectionFromEntitySet<DCustomer>("DCustomers");
            function.Parameter<Date>("modifiedDate");
            function.Parameter<TimeOfDay>("modifiedTime");
            function.Parameter<Date?>("nullableModifiedDate");
            function.Parameter<TimeOfDay?>("nullableModifiedTime");
        }

        private static void BuildActions(ODataModelBuilder builder)
        {
            ActionConfiguration action = builder.EntityType<DCustomer>().Action("BoundAction");
            action.Parameter<Date>("modifiedDate");
            action.Parameter<TimeOfDay>("modifiedTime");
            action.Parameter<Date?>("nullableModifiedDate");
            action.Parameter<TimeOfDay?>("nullableModifiedTime");
            action.CollectionParameter<Date>("dates");

            action = builder.Action("UnboundAction");
            action.Parameter<Date>("modifiedDate");
            action.Parameter<TimeOfDay>("modifiedTime");
            action.Parameter<Date?>("nullableModifiedDate");
            action.Parameter<TimeOfDay?>("nullableModifiedTime");
            action.CollectionParameter<Date>("dates");
        }

        private static Func<EntityInstanceContext, Uri> link = entityContext =>
        {
            object id;
            entityContext.EdmObject.TryGetPropertyValue("Id", out id);
            string uri = entityContext.Url.CreateODataLink(
                            new EntitySetPathSegment(entityContext.NavigationSource.Name),
                            new KeyValuePathSegment(id.ToString()));
            return new Uri(uri);
        };
    }
}
