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
            line.StrokeThickness = 0;
            p = testCrossLine(p);
            int pg = -1;
            int ptId = -1;
            Vector p1 = getPoint(p, out pg, out ptId, 5, false);
            if (ptId == -1)
            {
                if (line.Points.Count == 0)
                {
                    newLineProperty();
                    line.Points.Add(p.getPoint());
                }
                line.Points.Add(p.getPoint());
            }
            if (ptId == 0)
            {
                addPolygon();
            }
            line.StrokeThickness = 2;
        } 

        public static bool moveLastSegment(Vector p)
        {
            bool res = false;
            int pCount = line.Points.Count;
            if (pCount >= 1)
            {
                Vector testPoint = new Vector(line.Points[pCount - 1]);
                #region костыль
                if (testPoint.x == p.x) // исключает протыкание линии если новая линия горизонталь или вертикаль
                {
                    p.x += 0.1;
                }
                if (testPoint.y == p.y)
                {
                    p.y += 0.1;
                }
                #endregion
                line.Points.RemoveAt(pCount - 1);
                line.Points.Add(p.getPoint());
                line.StrokeThickness = 0;
                Vector p1 = testCrossLine(p);
                res = (p == p1);
                #region костыль
                if (!res)  // для того что бы точка находилась не на линии, а то самоперечечения становятся возможны
                {
                    double delta = 1;
                    if (p1.x < p.x)
                    {
                        p1.x -= delta;
                    }
                    else { p1.x += delta; }
                    if (p1.y < p.y)
                    {
                        p1.y -= delta;
                    }
                    else { p1.y += delta; }
                }
                #endregion
                line.Points.RemoveAt(pCount - 1);
                line.Points.Add(p1.getPoint());
                line.StrokeThickness = 2;
            }
            return res;
        }

        public static void movePolygonPoint(Vector p, int polygonId, int centerId)
        {
            List<Line> lines = new List<Line>();  // список линий полигона
            List<Vector> points = new List<Vector>(); // список точек полигона
            for (int i = 0; i < polygons[polygonId].Points.Count; i++)  // сохраняем точки для восстановления в случае необходимости
            {
                points.Add(new Vector(polygons[polygonId].Points[i]));
            }
            polygons[polygonId].Points[centerId] = new Point(p.x, p.y);  // перемещаем точку
            for (int i = 0; i < polygons[polygonId].Points.Count; i++) // составляем список линий полигона
            {
                int leftId = (i == polygons[polygonId].Points.Count - 1) ? 0 : i + 1;
                Line leftLine = getLine(new Vector(polygons[polygonId].Points[i]), new Vector(polygons[polygonId].Points[leftId]));  // левая линия от точки
                lines.Add(leftLine);
            }
            bool cross = false;  // флаг пересечений  (false - нет пересечений)
            for (int i = 0; i < lines.Count; i++)  // проверяем каждую линию с каждой (самый медленный алгоритм и самый простой)
            {
                for (int j = 0; j < lines.Count; j++) 
                {
                    int crossFlag = -2;
                    getCrossPoint(lines[i], lines[j], out crossFlag);
                    cross = cross || (crossFlag==2);
                }
            }
            if (cross)  // если пересечения восстанавливаем точки
            {
                polygons[polygonId].Points.Clear();
                for (int i = 0; i < points.Count; i++)
                {
                    polygons[polygonId].Points.Add(points[i].getPoint());
                }
            }
            else
            {
                setEllipseProperty(p, Brushes.Red); // рисуем курсорчик
            }
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

        public static Vector getPoint(Vector p, out int polygon, out int pNum, int radius, bool isPolygon)
        {
            polygon = -1;
            pNum = -1;
            if (isPolygon)
            {
                for (int i = 0; i < polygons.Count; i++)
                {
                    for (int j = 0; j < polygons[i].Points.Count; j++)
                    {
                        Vector ptv = new Vector(polygons[i].Points[j]);
                        if ((ptv - p).Lenght < radius)
                        {
                            pNum = j;
                            polygon = i;
                            setEllipseProperty(ptv, Brushes.Red);
                            return ptv;
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < line.Points.Count-1; j++)
                {
                    Vector ptv = new Vector(line.Points[j]);
                    if ((ptv - p).Lenght < radius)
                    {
                        pNum = j;
                        setEllipseProperty(ptv, Brushes.Green);
                        return ptv;
                    }
                }
            }
            setEllipseProperty(p, Brushes.Transparent);
            return new Vector();
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
