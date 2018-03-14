using System.Collections.Generic;

namespace WebStack.QA.Test.OData.ComplexTypeInheritance
{
    public class Window
    {
        public Window()
        {
            OptionalShapes = new List<Shape>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public Window Parent { get; set; }
        public Shape CurrentShape { get; set; }
        public IList<Shape> OptionalShapes { get; set; }
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
