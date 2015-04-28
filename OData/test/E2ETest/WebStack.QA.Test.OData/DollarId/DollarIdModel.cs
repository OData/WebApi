using System;
using System.Collections.Generic;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.DollarId
{
    /// <summary>
    /// EntityType "Singer"
    /// </summary>
    public class Singer
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string MasterPiece { get; set; }
        public List<Album> Albums { get; set; }
    }

    /// <summary>
    /// EntityType "Album"
    /// </summary>
    public class Album
    {
        public int ID { get; set; }
        public string Name { get; set; }

        [Contained]
        public List<AreaSales> Sales { get; set; }
        public Singer Singer { get; set; }
    }

    /// <summary>
    /// Contained EntityType "AreaSales"
    /// </summary>
    public class AreaSales
    {
        public int ID { get; set; }
        public String City { get; set; }
        public Int64 Sales { get; set; }
    }

    /// <summary>
    /// Define EdmModel
    /// </summary>
    internal class DollarIdEdmModel
    {
        public static IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var singerConfiguration = builder.EntitySet<Singer>("Singers");
            singerConfiguration.EntityType.Collection.Action("ResetDataSource");

            var albumConfiguration = builder.EntitySet<Album>("Albums");
            albumConfiguration.EntityType.Collection.Action("ResetDataSource");

            albumConfiguration.EntityType.Function("GetSingers").ReturnsCollectionFromEntitySet<Singer>("Singers").IsComposable = true;
            builder.Namespace = typeof(Singer).Namespace;

            return builder.GetEdmModel();
        }
    }
}
