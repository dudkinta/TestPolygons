using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TestPolygons
{
    static class Elements
    {
        public static int lastPoint = -1; // последняя точка незаконченного полигона
        public static int firstPoint = -1; // первая точка незаконченного полигона
        public static int counter; // идентификатор точки
        public static Dictionary<int, Line> currentPolygon = new Dictionary<int, Line>(); // недостроенный полигон
        public static Dictionary<int, Vector> points = new Dictionary<int, Vector>(); // коллекция точек
        public static Ellipse singlePoint = new Ellipse(); // для рисования начальной точки полигона
        public static List<Polygon> polygons = new List<Polygon>();  // коллекция полигонов

        public static List<UIElement> addPoint(Vector p) // добавление новой точки
        {
            counter++;
            List<UIElement> res = new List<UIElement>();
            points.Add(counter, p);
            if ((currentPolygon.Count == 0) && (lastPoint == -1))
            {
                singlePoint = getEllipse(p, 2);
            }
            else { singlePoint = new Ellipse(); }
            if (lastPoint != -1)
            {
                Line line = getLine(points[lastPoint], p);
                res.Add(line);
                line.Tag = counter - 1;
                currentPolygon.Add(counter, line);
            }
            else { firstPoint = counter; }
            lastPoint = counter;
            return res;
        }

        public static Polygon addPolygon(Dictionary<int, Line> lines) // постройка нового полигона
        {
            Line line = getLine(points[lastPoint], points[firstPoint]);
            lastPoint = -1;
            currentPolygon.Add(lastPoint, line);
            Polygon res = new Polygon();
            List<int> indexs = new List<int>();
            foreach(KeyValuePair<int, Line> ln in lines)
            {
                res.Points.Add(new Point(ln.Value.X1,ln.Value.Y1));
                indexs.Add(ln.Key);
            }
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(200, 127, 127, 127);
            res.Fill = mySolidColorBrush;
            res.Stroke = System.Windows.Media.Brushes.Black;
            res.StrokeThickness = 2;
            res.Tag = indexs;
            firstPoint = -1;
            polygons.Add(res);
            currentPolygon.Clear();
            return res;
        }
        
        public static int getIndexPoint(Vector p) // поиск индекса точки
        {
            int res = -1;
            foreach (KeyValuePair<int,Vector> point in points)
            {
                if (((point.Value.x - p.x < 5) && (point.Value.x - p.x > -5)) && ((point.Value.y- p.y < 5) && (point.Value.y - p.y > -5)))
                {
                    return point.Key;
                }
            }
            return res;
        }

        public static Line getLine(double x1, double y1, double x2, double y2) //создание линии по 2-м точкам заданными координатами
        {
            Line res = new Line();
            res.X1 = x1;
            res.X2 = x2;
            res.Y1 = y1;
            res.Y2 = y2;
            res.Stroke = System.Windows.Media.Brushes.Black;
            res.StrokeThickness = 2;
            return res;
        }

        public static Line getLine(Vector p1, Vector p2) //сохдание линии по 2-м точкам заданными векторами
        {
            return getLine(p1.x, p1.y, p2.x, p2.y);
        }

        public static Ellipse getEllipse(double x, double y, int thickness) //создание эллипса по координатам
        {
            Ellipse res = new Ellipse();
            res.Width = thickness * 2 + 1;
            res.Height = thickness * 2 + 1;
            res.Stroke = System.Windows.Media.Brushes.Black;
            res.StrokeThickness = thickness;
            res.Margin = new Thickness(x - (thickness + 0.5), y - (thickness + 0.5), 0, 0);
            return res;
        }

        public static Ellipse getEllipse(Vector p) // создание эллипса по вектору со стандартной толщиной (для начальной точки)
        {
            return getEllipse(p.x, p.y, 2);
        }

        public static Ellipse getEllipse(Vector p, int thickness) // создание эллипса по вектору с нестандартной толщиной (для выбраной точки)
        {
            return getEllipse(p.x, p.y, thickness);
        }
        
        public static void removePoint(int removeIndex) // удаление точки из полигона
        {
            for (int i = 0; i < polygons.Count; i++)
            {
                Polygon polygon = polygons[i];
                if (polygon.Points.Count > 3)  // удаление точки и перестройка полигона
                {
                    for (int j = 0; j < polygon.Points.Count; j++)
                    {
                        if ((polygon.Points[j].X == points[removeIndex].x) && (polygon.Points[j].Y == points[removeIndex].y))
                        {
                            polygon.Points.RemoveAt(j);
                            List<int> indexs = (List<int>)polygon.Tag;
                            indexs.Remove(j);
                        }
                    }
                    points.Remove(removeIndex);
                }
            }
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

        public static bool isCrossLines(Line l1, Line l2) // проверка пересечения линий
        {
            // Ax + By + C = 0   - уравнение прямой
            double a1 = l1.Y2 - l1.Y1;
            double a2 = l2.Y2 - l2.Y1;
            double b1 = l1.X2 - l1.X1;
            double b2 = l2.X2 - l2.X1;
            double c1 = l1.X1 * l1.Y2 - l1.X2 * l1.Y1;
            double c2 = l1.X1 * l1.Y2 - l1.X2 * l1.Y1;
            if (a1 * b2 - a2 * b1 == 0)
            {
                #region Линии параллельны
                if ((a1 * c2 - c1 * a2 == 0) && (b1 * c2 - c1 * b2 == 0))  //проверка через 2 определителя матрицы
                {
                    #region  Линии на одной прямой
                    double x1 = Math.Min(l1.X1, l1.X2);
                    double x2 = Math.Max(l1.X1, l1.X2);
                    double x3 = Math.Min(l2.X1, l2.X2);
                    double x4 = Math.Max(l2.X1, l2.X2);
                    if (x1 <= x3)
                    {
                        //первая линия левее или начало в одной точке
                        if (x3 <= x2)
                        {
                            //линии имеют общий отрезок или точка конца первой совпадает с точкой начала второй
                            return true;
                        }
                        else
                        {
                            // линии имеют расстояние между точкой конца первой и точкой начала второй
                            return false;
                        }
                    }
                    else
                    {
                        //первая линия правее
                        if (x4 <= x1)
                        {
                            //линии имеют общий отрезок или точка конца второй совпадает с точкой начала первой
                            return true;
                        }
                        else
                        {
                            // линии имеют расстояние между точкой конца второй и точкой начала первой
                            return false;
                        }
                    }
                    #endregion
                }
                else { return false; }
                #endregion
            }
            #region Линии пересекаются
            else
            {
                // x,y - точка пересечения
                double x = -(c1 * b2 - c2 * b1) / (a1 * b2 - a2 * b1);
                double y = -(a1 * c2 - a2 * c1) / (a1 * b2 - a2 * b1);
                double x1 = Math.Min(l1.X1, l1.X2);
                double x2 = Math.Max(l1.X1, l1.X2);
                if ((x1 <= x) && (x2 > x))
                {
                    // точка пересечения на отрезке
                    return true;
                }
                else { return false; }
            }
            #endregion
        } 
    }
}
