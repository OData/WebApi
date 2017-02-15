using System;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.DateTimeOffsetSupport
{
    public class DateTimeOffsetEdmModel
    {
        public static IEdmModel GetExplicitModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var fileType = builder.EntityType<File>().HasKey(f => f.FileId);
            fileType.Property(f => f.Name);
            fileType.Property(f => f.CreatedDate);
            fileType.Property(f => f.DeleteDate);

            var files = builder.EntitySet<File>("Files");
            files.HasIdLink(link, true);
            files.HasEditLink(link, true);

            BuildFunctions(builder);
            BuildActions(builder);

            return builder.GetEdmModel();
        }

        public static IEdmModel GetConventionModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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

        private static Func<EntityInstanceContext, Uri> link = entityContext =>
        {
            object id;
            entityContext.EdmObject.TryGetPropertyValue("FileId", out id);
            string uri = entityContext.Url.CreateODataLink(
                            new EntitySetPathSegment(entityContext.NavigationSource.Name),
                            new KeyValuePathSegment(id.ToString()));
            return new Uri(uri);
        };
    }
}
