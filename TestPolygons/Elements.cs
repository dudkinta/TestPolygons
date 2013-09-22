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
        public static Line currentLine = new Line();
        public static List<Polygon> polygons = new List<Polygon>();  // коллекция полигонов

        public static List<UIElement> addPoint(Vector p) // добавление новой точки
        {
            counter++;
            List<UIElement> res = new List<UIElement>();
            points.Add(counter, p);
            if ((currentPolygon.Count == 0) && (lastPoint == -1))
            {
                singlePoint = getEllipse(p, 2);
                currentLine = getLine(p, p);
                currentLine.StrokeThickness = 1;
            }
            else 
            { 
                singlePoint = new Ellipse();
            }
            if (lastPoint != -1)
            {
                Line line = getLine(points[lastPoint], p);
                res.Add(line);
                line.Tag = counter - 1;
                currentPolygon.Add(counter, line);
                currentLine.X1 = p.x;
                currentLine.Y1 = p.y;
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
            currentLine.StrokeThickness = 0;
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

        public static Vector checkCurrentPolygon(Vector res, Line testLine)
        {
            foreach (KeyValuePair<int, Line> polygonLine  in currentPolygon)
            {
                Line l1 = polygonLine.Value;  // для укорочения записи
                Line l2 = testLine;
                int crossFlag = -2;
                Vector c = new Vector(l2.X1, l2.Y1);
                Vector d = new Vector(l2.X2, l2.Y2);
                Vector cross = getCrossPoint(l1, l2, out crossFlag);
                if (crossFlag == 2)
                {
                    if ((cross != c) && (cross != d))
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

        public static Line decLenght(Line line, double delta)
        {
            if (line.X1 < line.X2)
            {
                line.X2 -= delta;
            }
            else line.X2 += delta;
            if (line.Y1 < line.Y2)
            {
                line.Y2 -= delta;
            }
            else line.Y2 += delta;
            return line;
        }
    }
}
