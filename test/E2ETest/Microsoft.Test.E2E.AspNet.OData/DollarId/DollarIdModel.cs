//-----------------------------------------------------------------------------
// <copyright file="DollarIdModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.DollarId
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
        public static IEdmModel GetModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
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
