//-----------------------------------------------------------------------------
// <copyright file="ComplexTypeInheritanceDataModels.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.ComplexTypeInheritance
{
    public class Window
    {
        public Window()
        {
            OptionalShapes = new List<Shape>();
            PolygonalShapes = new List<Polygon>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public Window Parent { get; set; }
        public Shape CurrentShape { get; set; }
        public IList<Shape> OptionalShapes { get; set; }
        public IList<Polygon> PolygonalShapes { get; set; }
    }

    public abstract class Shape
    {
        public bool HasBorder { get; set; }
    }

    public class Circle : Shape
    {
        public Point Center { get; set; }
        public int Radius { get; set; }

        public override string ToString()
        {
            // {centerX, centerY,radius}
            return "{" + Center.X + "," + Center.Y + "," + Radius + "}";
        }
    }

    public class Polygon : Shape
    {
        public IList<Point> Vertexes { get; set; }
        public Polygon()
        {
            Vertexes = new List<Point>();
        }
    }

    public class Rectangle : Polygon
    {
        public Point TopLeft { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Rectangle()
        {

        }

        public Rectangle(Point topLeft, int width, int height)
        {
            TopLeft = topLeft;
            Width = width;
            Height = height;

            this.Fill();
        }

        public void Fill()
        {
            if(Width==0||Height==0)
            {
                return;
            }
            Vertexes.Add(TopLeft);
            Vertexes.Add(new Point()
            {
                X = TopLeft.X + Width,
                Y = TopLeft.Y,
            });

            Vertexes.Add(new Point()
            {
                X = TopLeft.X + Width,
                Y = TopLeft.Y + Height,
            });

            Vertexes.Add(new Point()
            {
                X = TopLeft.X,
                Y = TopLeft.Y + Height,
            });
        }
    }

    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
