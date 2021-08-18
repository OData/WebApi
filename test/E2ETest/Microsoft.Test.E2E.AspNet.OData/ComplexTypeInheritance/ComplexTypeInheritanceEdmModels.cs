//-----------------------------------------------------------------------------
// <copyright file="ComplexTypeInheritanceEdmModels.cs" company=".NET Foundation">
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

namespace Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance
{
    public class ComplexTypeInheritanceEdmModels
    {
        public static IEdmModel GetExplicitModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Window> windowType = builder.EntityType<Window>();
            windowType.HasKey(a => a.Id);
            windowType.Property(a => a.Name).IsRequired();
            windowType.ComplexProperty(w => w.CurrentShape).IsOptional();
            windowType.CollectionProperty(w => w.OptionalShapes);
            windowType.CollectionProperty(w => w.PolygonalShapes);
            windowType.HasOptional<Window>(w => w.Parent);

            ComplexTypeConfiguration<Shape> shapeType = builder.ComplexType<Shape>();
            shapeType.Property(s => s.HasBorder);
            shapeType.Abstract();

            ComplexTypeConfiguration<Circle> circleType = builder.ComplexType<Circle>();
            circleType.ComplexProperty(c => c.Center);
            circleType.Property(c => c.Radius);
            circleType.DerivesFrom<Shape>();

            ComplexTypeConfiguration<Polygon> polygonType = builder.ComplexType<Polygon>();
            polygonType.CollectionProperty(p => p.Vertexes);
            polygonType.DerivesFrom<Shape>();

            ComplexTypeConfiguration<Rectangle> rectangleType = builder.ComplexType<Rectangle>();
            rectangleType.ComplexProperty(r => r.TopLeft);
            rectangleType.Property(r => r.Width);
            rectangleType.Property(r => r.Height);
            rectangleType.DerivesFrom<Polygon>();

            ComplexTypeConfiguration<Point> pointType = builder.ComplexType<Point>();
            pointType.Property(p => p.X);
            pointType.Property(p => p.Y);

            EntitySetConfiguration<Window> windows = builder.EntitySet<Window>("Windows");
            windows.HasEditLink(link, true);
            windows.HasIdLink(link, true);
            windows.HasOptionalBinding(c => c.Parent, "Windows");

            builder.Namespace = typeof(Window).Namespace;

            return builder.GetEdmModel();
        }

        public static IEdmModel GetConventionModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<Window>("Windows");
            builder.Namespace = typeof(Window).Namespace;

            return builder.GetEdmModel();
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
    }
}
