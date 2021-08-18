//-----------------------------------------------------------------------------
// <copyright file="DateTimeEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;

namespace Microsoft.Test.E2E.AspNet.OData.DateTimeSupport
{
    public class DateTimeEdmModel
    {
        public static IEdmModel GetExplicitModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var fileType = builder.EntityType<File>().HasKey(f => f.FileId);
            fileType.Property(f => f.Name);
            fileType.Property(f => f.CreatedDate);
            fileType.Property(f => f.DeleteDate);
            fileType.CollectionProperty(f => f.ModifiedDates);

            var files = builder.EntitySet<File>("Files");
            files.HasIdLink(link, true);
            files.HasEditLink(link, true);

            BuildFunctions(builder);
            BuildActions(builder);

            return builder.GetEdmModel();
        }

        public static IEdmModel GetConventionModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<File>("Files");

            BuildFunctions(builder);
            BuildActions(builder);

            return builder.GetEdmModel();
        }

        private static void BuildFunctions(ODataModelBuilder builder)
        {
            FunctionConfiguration function =
                builder.EntityType<File>()
                    .Collection.Function("GetFilesModifiedAt")
                    .ReturnsCollectionViaEntitySetPath<File>("bindingParameter");
            function.Parameter<DateTime>("modifiedDate");
        }

        private static void BuildActions(ODataModelBuilder builder)
        {
            builder.EntityType<File>().Action("CopyFiles").Parameter<DateTime>("createdDate");

            builder.Action("ResetDataSource");
        }

        private static Func<ResourceContext, Uri> link = entityContext =>
        {
            object id;
            entityContext.EdmObject.TryGetPropertyValue("FileId", out id);
            string uri = entityContext.GetUrlHelper().CreateODataLink(
                            new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                            new KeySegment(new[] { new KeyValuePair<string, object>("FileId", id) }, entityContext.StructuredType as IEdmEntityType, null));
            return new Uri(uri);
        };
    }
}
