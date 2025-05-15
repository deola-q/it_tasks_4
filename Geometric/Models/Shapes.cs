using System;

namespace Geometric.Models
{
    // Базовый класс для геометрической фигуры
    public abstract class Shape : IShape
    {
        public double X { get; set; }
        public double Y { get; set; }

        protected Shape(double x, double y)
        {
            X = x;
            Y = y;
        }

        public abstract (double Left, double Top, double Right, double Bottom) BoundingBox { get; }

        public string BoundingBoxString => $"(X левый: {BoundingBox.Left}, Y верхний: {BoundingBox.Top}, X правый: {BoundingBox.Right}, Y нижний: {BoundingBox.Bottom})";

        public virtual double Area => 0;

        public override string ToString()
        {
            return $"Shape (X: {X}, Y: {Y})";
        }
    }

    // Класс для точки
    public class Point : Shape
    {
        public Point(double x, double y) : base(x, y) { }

        public override (double Left, double Top, double Right, double Bottom) BoundingBox => (X, Y, X, Y);

        public override string ToString()
        {
            return $"Точка (X: {X}, Y: {Y})";
        }
    }

    // Класс для линии
    public class Line : Shape
    {
        public double X2 { get; set; }
        public double Y2 { get; set; }

        public Line(double x1, double y1, double x2, double y2) : base(x1, y1)
        {
            X2 = x2;
            Y2 = y2;
        }

        public override (double Left, double Top, double Right, double Bottom) BoundingBox
        {
            get
            {
                double left = Math.Min(X, X2);
                double top = Math.Max(Y, Y2);
                double right = Math.Max(X, X2);
                double bottom = Math.Min(Y, Y2);
                return (left, top, right, bottom);
            }
        }

        public override string ToString()
        {
            return $"Линия (X1: {X}, Y1: {Y}; X2: {X2}, Y2: {Y2})";
        }
    }

    // Класс для многоугольника
    public class Polygon : Shape
    {
        public double[] VerticesX { get; set; }
        public double[] VerticesY { get; set; }

        public Polygon(double[] verticesX, double[] verticesY) : base(0, 0)
        {
            VerticesX = verticesX;
            VerticesY = verticesY;

            double sumX = 0, sumY = 0;
            for (int i = 0; i < verticesX.Length; i++)
            {
                sumX += verticesX[i];
                sumY += verticesY[i];
            }
            X = sumX / verticesX.Length;
            Y = sumY / verticesY.Length;
        }

        public override (double Left, double Top, double Right, double Bottom) BoundingBox
        {
            get
            {
                double left = VerticesX[0];
                double top = VerticesY[0];
                double right = VerticesX[0];
                double bottom = VerticesY[0];

                for (int i = 1; i < VerticesX.Length; i++)
                {
                    if (VerticesX[i] < left) left = VerticesX[i];
                    if (VerticesX[i] > right) right = VerticesX[i];
                    if (VerticesY[i] > top) top = VerticesY[i]; 
                    if (VerticesY[i] < bottom) bottom = VerticesY[i];
                }

                return (left, top, right, bottom);
            }
        }

        public override double Area
        {
            get
            {
                double area = 0;
                int n = VerticesX.Length;

                for (int i = 0; i < n; i++)
                {
                    int j = (i + 1) % n;
                    area += VerticesX[i] * VerticesY[j];
                    area -= VerticesX[j] * VerticesY[i];
                }

                return Math.Abs(area) / 2;
            }
        }

        public override string ToString()
        {
            return $"Многоугольник (Центр: X={X}, Y={Y}, Число вершин: {VerticesX.Length})";
        }
    }

    // Класс для эллипса
    public class Ellipse : Shape
    {
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }

        public Ellipse(double x, double y, double radiusX, double radiusY) : base(x, y)
        {
            RadiusX = radiusX;
            RadiusY = radiusY;
        }

        public override (double Left, double Top, double Right, double Bottom) BoundingBox
        {
            get
            {
                return (X - RadiusX, Y + RadiusY, X + RadiusX, Y - RadiusY);
            }
        }

        public override double Area => Math.PI * RadiusX * RadiusY;

        public override string ToString()
        {
            return $"Эллипс (Центр: X={X}, Y={Y}, Радиус по X={RadiusX}, Радиус по Y={RadiusY})";
        }
    }
}

