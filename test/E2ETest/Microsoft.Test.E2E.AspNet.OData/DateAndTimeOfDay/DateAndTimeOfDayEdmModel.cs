//-----------------------------------------------------------------------------
// <copyright file="DateAndTimeOfDayEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using EdmPrimitiveTypeKind = Microsoft.OData.Edm.EdmPrimitiveTypeKind;
using IEdmModel = Microsoft.OData.Edm.IEdmModel;

namespace Microsoft.Test.E2E.AspNet.OData.DateAndTimeOfDay
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

        public static IEdmModel GetConventionModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<DCustomer>("DCustomers");

            BuildFunctions(builder);
            BuildActions(builder);

            builder.EntitySet<EfCustomer>("EfCustomers");

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

            // just for reset the data source
            builder.Action("ResetDataSource");
        }

        private static Func<ResourceContext, Uri> link = entityContext =>
        {
            object id;
            entityContext.EdmObject.TryGetPropertyValue("Id", out id);
            string uri = ResourceContextHelper.CreateODataLink(entityContext,
                            new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                            new KeySegment(new[] { new KeyValuePair<string, object>("Id", id) }, entityContext.StructuredType as IEdmEntityType, null));
            return new Uri(uri);
        };

        public static IEdmModel BuildEfPersonEdmModel()
        {
            string Namespace = typeof(EfPerson).Namespace;

            EdmModel model = new EdmModel();

            EdmEntityType person = new EdmEntityType(Namespace, "Person");
            person.AddKeys(person.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32, isNullable: false));
            person.AddStructuralProperty("Birthday", EdmPrimitiveTypeKind.Date, isNullable: true);
            model.AddElement(person);

            EdmEntityContainer container = new EdmEntityContainer(Namespace, "Default");
            container.AddEntitySet("EfPeople", person);

            model.AddElement(container);
            model.SetAnnotationValue<ClrTypeAnnotation>(person, new ClrTypeAnnotation(typeof(EfPerson)));

            return model;
        }
    }
}
