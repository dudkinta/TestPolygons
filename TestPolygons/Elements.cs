using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TestPolygons
{
    static class Elements
    {
        public static Ellipse currentPoint = new Ellipse();  // маркер выбраной точки
        public static List<Polygon> polygons = new List<Polygon>();  // коллекция полигонов
        public static Polyline line = new Polyline(); // недорисованный полигон

        public static void addPoint(Vector p)
        {
            if (line.Points.Count == 0)
            {
                newLineProperty();
                line.Points.Add(p.getPoint());
            }
            line.StrokeThickness = 0;
            p = testCrossLine(p);
            line.Points.Add(p.getPoint());
            line.StrokeThickness = 2;
        }

        public static bool moveLastSegment(Vector p)
        {
            bool res = false;
            int pCount = line.Points.Count;
            if (pCount > 0)
            {
                line.Points.RemoveAt(pCount - 1);
                line.Points.Add(p.getPoint());
                line.StrokeThickness = 0;
                Vector p1 = testCrossLine(p);
                res = (p == p1);
                Vector a = new Vector(p1);
                Vector b = new Vector(line.Points[pCount - 2]);
                if (a.x < b.x)
                {
                    a.x += 1;
                }
                else { a.x -= 1; }
                if (a.y < b.y)
                {
                    a.y += 1;
                }
                else { a.y -= 1; }
                line.Points.RemoveAt(pCount - 1);
                line.Points.Add(a.getPoint());

                line.StrokeThickness = 2;
            }
            return res;
        }

        public static void deleteLastPoint()
        {
            int pCount = line.Points.Count;
            if (pCount > 0)
            {
                line.Points.RemoveAt(pCount - 1);
            }
        }

        public static bool addPolygon()
        {
            bool res = false;
            int pCount = line.Points.Count;
            if (pCount > 3)
            {
                Vector p = new Vector(line.Points[0]);
                bool test = moveLastSegment(p);
                if (test)
                {
                    line.Points.RemoveAt(pCount - 1);
                    Polygon polygon = new Polygon();
                    polygon.Points = line.Points;
                    SolidColorBrush sbrush = new SolidColorBrush(Color.FromArgb(200, 127, 127, 127));
                    polygon.Fill = sbrush;
                    polygon.StrokeThickness = 2;
                    polygon.Stroke = Brushes.Blue;
                    polygons.Add(polygon);
                    line = new Polyline();
                    res = true;
                }
            }
            return res;
        }

        private static void newLineProperty()
        {
            line.Stroke = Brushes.Black;
            line.StrokeThickness = 2;
        }

        private static void setEllipseProperty(Vector p, Brush color)
        {
            currentPoint.Width = 10;
            currentPoint.Height = 10;
            currentPoint.StrokeThickness = 5;
            currentPoint.Stroke = color;
            currentPoint.Margin = new Thickness(p.x - 5, p.y - 5, 0, 0);
        }

        public static Vector getPoint(Vector p, out int polygon, out int pNum)
        {
            polygon = -1;
            pNum = -1;
            for (int i=0;i<polygons.Count;i++)
            {
                for (int j=0;j<polygons[i].Points.Count;j++)
                {
                    Vector ptv = new Vector(polygons[i].Points[j]);
                    if ((ptv - p).Lenght < 5)
                    {
                        pNum = j;
                        polygon = i;
                        setEllipseProperty(ptv, Brushes.Red);
                        return ptv;
                    }
                }
            }
            setEllipseProperty(p, Brushes.Transparent);
            return null;
        }

        private static Vector testCrossLine(Vector r)
        {
            Vector res = new Vector(r);
            int pCount = line.Points.Count;
            if (pCount > 1)
            {
                for (int i = 0; i < pCount - 1; i++)
                {
                    Vector a = new Vector(line.Points[i]);
                    Vector b = new Vector(line.Points[i + 1]);
                    Vector c = new Vector(line.Points[pCount - 2]);
                    Vector d = new Vector(line.Points[pCount-1]);
                    Line l1 = getLine(a, b);
                    Line l2 = getLine(c, d);
                    int crossFlag = -2;
                    Vector cross = getCrossPoint(l1, l2, out crossFlag);
                    if (crossFlag == 2)
                    {
                        if ((cross - c).Lenght < (res - c).Lenght)
                        {
                            res = cross;
                        }
                    }
                }
            }
            return res;
        }

        private static Line getLine(Vector p1, Vector p2)
        {
            Line res = new Line();
            res.X1 = p1.x; res.Y1 = p1.y;
            res.X2 = p2.x; res.Y2 = p2.y;
            return res;
        }

        public static double getLenght(Vector a, Vector b, Vector c) // вычисление расстояние от точки до отрезка
        {
            //a - начало отрезка
            //б - конец отрезка 
            //с - точка
            double p = (c - a) * (b - a);
            double r = (b - a) * (b - a);
            if (0 >= p) { return (c - a).Lenght; }
            if (p >= r) { return (c - b).Lenght; }
            if ((0 < p) && (p < r)) { return (c - a - p / r * (b - a)).Lenght; }
            return double.MaxValue;
        }

        public static Vector getCrossPoint(Line l1, Line l2, out int crossFlag)  // тут мне помогал гугль и Крамер
        {
            crossFlag = -2;
            Vector result = new Vector();
            double m = ((l2.X2 - l2.X1) * (l1.Y1 - l2.Y1) - (l2.Y2 - l2.Y1) * (l1.X1 - l2.X1));
            double w = ((l1.X2 - l1.X1) * (l1.Y1 - l2.Y1) - (l1.Y2 - l1.Y1) * (l1.X1 - l2.X1));
            double n = ((l2.Y2 - l2.Y1) * (l1.X2 - l1.X1) - (l2.X2 - l2.X1) * (l1.Y2 - l1.Y1));
            double Ua = m / n;
            double Ub = w / n;
            if ((n == 0) && (m != 0))
            {
                crossFlag = -1; //Прямые параллельны и не имеют пересечения
            }
            else if ((m == 0) && (n == 0))
            {
                crossFlag = 0; //Прямые совпадают
            }
            else
            {
                //Прямые имеют точку пересечения 
                result.x = l1.X1 + Ua * (l1.X2 - l1.X1);
                result.y = l1.Y1 + Ua * (l1.Y2 - l1.Y1);
                crossFlag = 1;
                double x11 = Math.Min(l2.X1, l2.X2);
                double x12 = Math.Max(l2.X1, l2.X2);
                double y11 = Math.Min(l2.Y1, l2.Y2);
                double y12 = Math.Max(l2.Y1, l2.Y2);

                double x21 = Math.Min(l1.X1, l1.X2);
                double x22 = Math.Max(l1.X1, l1.X2);
                double y21 = Math.Min(l1.Y1, l1.Y2);
                double y22 = Math.Max(l1.Y1, l1.Y2);
                if ((result.x > x11) && (result.x < x12) && (result.y > y11) && (result.y < y12) && (result.x > x21) && (result.x < x22) && (result.y > y21) && (result.y < y22))
                {
                    crossFlag = 2;  // отрезки имеют точку пересечения
                }
            }
            return result;
        }

    }
}
