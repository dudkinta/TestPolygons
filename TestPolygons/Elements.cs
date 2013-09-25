using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

namespace TestPolygons
{
    static class Elements
    {
        public static string debug = "";

        public static Ellipse currentPoint = new Ellipse();  // маркер выбраной точки

        public static List<Polygon> polygons = new List<Polygon>();  // коллекция полигонов

        public static List<Line> unionLines = new List<Line>();  // линии объединенного полигона

        public static ObservableCollection<Canvas> plgns = new ObservableCollection<Canvas>();  // коллекция полигонов

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
            List<Vector> points = savePolygonPoints(polygons[polygonId]);
            #region костыль
            // опять костыль с протыканием вертикали и горизонтали
            if (polygons[polygonId].Points[centerId].X == p.x)
            {
                p.x += 0.1;
            }
            if (polygons[polygonId].Points[centerId].Y == p.y)
            {
                p.y += 0.1;
            }
            #endregion
            polygons[polygonId].Points[centerId] = new Point(p.x, p.y);  // перемещаем точку
            if (testListLines(polygons[polygonId]))  // если пересечения восстанавливаем точки
            {
                restorePolygonPoints(points, polygons[polygonId]);
            }
            else
            {
                sendToCollect(polygonId);
                currentPoint = setEllipseProperty(currentPoint, p, Brushes.Red, 5); // рисуем курсорчик
            }
            unionPolygon();
        }

        private static bool testListLines(Polygon pg)
        {
            List<Line> lines = getLinesPolygon(pg);
            bool cross = false;  // флаг пересечений  (false - нет пересечений)
            for (int i = 0; i < lines.Count; i++)  // проверяем каждую линию с каждой (самый медленный алгоритм и самый простой)
            {
                for (int j = 0; j < lines.Count; j++)
                {
                    int crossFlag = -2;
                    getCrossPoint(lines[i], lines[j], out crossFlag, false);
                    cross = cross || (crossFlag == 2);
                }
            }
            return cross;
        }

        public static List<Vector> savePolygonPoints(Polygon pg)
        {
            List<Vector> points = new List<Vector>(); // список точек полигона
            for (int i = 0; i < pg.Points.Count; i++)  // сохраняем точки для восстановления в случае необходимости
            {
                points.Add(new Vector(pg.Points[i]));
            }
            return points;
        }

        public static void restorePolygonPoints(List<Vector> points, Polygon pg)
        {
            pg.Points.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                pg.Points.Add(points[i].getPoint());
            }
        }

        private static List<Line> getLinesPolygon(Polygon pg)
        {
            List<Line> lines = new List<Line>();  // список линий полигона
            for (int i = 0; i < pg.Points.Count; i++) // составляем список линий полигона
            {
                int leftId = (i == pg.Points.Count - 1) ? 0 : i + 1;
                Line leftLine = getLine(new Vector(pg.Points[i]), new Vector(pg.Points[leftId]));  // левая линия от точки
                lines.Add(leftLine);
            }
            return lines;
        }

        public static void deleteLastPoint()
        {
            int pCount = line.Points.Count;
            if (pCount > 0)
            {
                line.Points.RemoveAt(pCount - 1);
            }
        }

        public static bool deletePolygonPoint(int pg, int pId)
        {
            if (polygons[pg].Points.Count == 3)
            {
                polygons.RemoveAt(pg);
                plgns.RemoveAt(pg);
                return true;
            }
            else
            {

                List<Vector> points = savePolygonPoints(polygons[pg]);
                polygons[pg].Points.RemoveAt(pId);  // удаляем точку
                if (testListLines(polygons[pg]))  // если пересечения восстанавливаем точки
                {
                    restorePolygonPoints(points, polygons[pg]);
                    return false;
                }
                sendToCollect(pg);
                unionPolygon();
                currentPoint.Stroke = Brushes.Transparent;
            }
            return true;
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
                    SolidColorBrush sbrush = new SolidColorBrush(Color.FromArgb(50, 127, 127, 127));
                    polygon.Fill = sbrush;
                    polygon.StrokeThickness = 0;
                    polygon.Stroke = Brushes.Transparent;
                    polygons.Add(polygon);
                    line = new Polyline();
                    res = true;
                    addToCollect(polygon);
                    unionPolygon();
                }
            }
            return res;
        }

        public static void addToCollect(Polygon pg)
        {
            List<Vector> points = normalizePoints(savePolygonPoints(pg), 130, 130);
            Polygon newPG = new Polygon();
            restorePolygonPoints(points, newPG);
            SolidColorBrush sbrush = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127));
            newPG.Fill = sbrush;
            newPG.StrokeThickness = 1;
            newPG.Stroke = Brushes.Blue;
            Canvas cnv = new Canvas();
            cnv.Children.Add(newPG);
            plgns.Add(cnv);
        }

        private static void sendToCollect(int pg)
        {
            List<Vector> points = normalizePoints(savePolygonPoints(polygons[pg]), 130, 130);
            Polygon newPG = new Polygon();
            restorePolygonPoints(points, newPG);
            SolidColorBrush sbrush = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127));
            newPG.Fill = sbrush;
            newPG.StrokeThickness = 1;
            newPG.Stroke = Brushes.Blue;
            plgns[pg].Children.Clear();
            plgns[pg].Children.Add(newPG);
        }

        private static List<Vector> normalizePoints(List<Vector> points, int normX, int normY)
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            for (int i = 0; i < points.Count; i++)
            {
                minX = (points[i].x < minX) ? points[i].x : minX;
                minY = (points[i].y < minY) ? points[i].y : minY;
                maxX = (points[i].x > maxX) ? points[i].x : maxX;
                maxY = (points[i].y > maxY) ? points[i].y : maxY;
            }
            double scale = ((maxX - minX) > (maxY - minY)) ? normX / ((maxX - minX)) : normY / (maxY - minY);
            double sx = ((maxX - minX) > (maxY - minY)) ? 0 : (normX - (maxX - minX) * scale) / 2;
            double sy = ((maxX - minX) > (maxY - minY)) ? (normY - (maxY - minY) * scale) / 2 : 0;
            for (int i = 0; i < points.Count; i++)
            {
                points[i].x = points[i].x - minX;
                points[i].x = (points[i].x * scale) + sx;
                points[i].y = points[i].y - minY;
                points[i].y = (points[i].y * scale) + sy;
            }
            return points;
        }

        public static bool addPointPolygon(Vector p)
        {
            double minLenght = double.MaxValue;
            int polygonId = -1;
            int lineId = -1;
            for (int i = 0; i < polygons.Count; i++)
            {
                List<Line> lines = getLinesPolygon(polygons[i]);
                for (int j = 0; j < lines.Count; j++)
                {
                    double len = getLenght(lines[j], p);
                    if (len < minLenght)
                    {
                        minLenght = len;
                        polygonId = i;
                        lineId = j;
                    }
                }
            }
            if (polygonId != -1)
            {
                List<Vector> points = savePolygonPoints(polygons[polygonId]);
                polygons[polygonId].Points.Insert(lineId + 1, p.getPoint()); // добавляем точку
                if (testListLines(polygons[polygonId]))  // если пересечения восстанавливаем точки
                {
                    restorePolygonPoints(points, polygons[polygonId]);
                    return true;
                }
                sendToCollect(polygonId);
                unionPolygon();
            }
            return false;
        }

        private static void newLineProperty()
        {
            line.Stroke = Brushes.Black;
            line.StrokeThickness = 2;
        }

        private static Ellipse setEllipseProperty(Ellipse el, Vector p, Brush color, int radius)
        {
            el.Width = radius * 2;
            el.Height = radius * 2;
            el.StrokeThickness = 5;
            el.Stroke = color;
            el.Margin = new Thickness(p.x - radius, p.y - radius, 0, 0);
            return el;
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
                            currentPoint = setEllipseProperty(currentPoint, ptv, Brushes.Red, 5);
                            return ptv;
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < line.Points.Count - 1; j++)
                {
                    Vector ptv = new Vector(line.Points[j]);
                    if ((ptv - p).Lenght < radius)
                    {
                        pNum = j;
                        currentPoint = setEllipseProperty(currentPoint, ptv, Brushes.Green, 5);
                        return ptv;
                    }
                }
            }
            currentPoint = setEllipseProperty(currentPoint, p, Brushes.Transparent, 5);
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
                    Vector d = new Vector(line.Points[pCount - 1]);
                    Line l1 = getLine(a, b);
                    Line l2 = getLine(c, d);
                    int crossFlag = -2;
                    Vector cross = getCrossPoint(l1, l2, out crossFlag, false);
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

        private static double getLenght(Line line, Vector c) // вычисление расстояние от точки до отрезка
        {
            Vector a = new Vector();
            a.x = line.X1;
            a.y = line.Y1;
            Vector b = new Vector();
            b.x = line.X2;
            b.y = line.Y2;
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

        public static Vector getCrossPoint(Line l1, Line l2, out int crossFlag, bool hard)  // тут мне помогал гугль и Крамер
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
                if (!hard)
                {
                    if ((result.x > x11) && (result.x < x12) && (result.y > y11) && (result.y < y12) && (result.x > x21) && (result.x < x22) && (result.y > y21) && (result.y < y22))
                    {
                        crossFlag = 2;  // отрезки имеют точку пересечения
                    }
                }
                else
                {
                    if ((result.x >= x11) && (result.x <= x12) && (result.y >= y11) && (result.y <= y12) && (result.x >= x21) && (result.x <= x22) && (result.y >= y21) && (result.y <= y22))
                    {
                        crossFlag = 2;  // отрезки имеют точку пересечения
                    }
                }
            }
            return result;
        }

        public static Dictionary<string, List<Vector>> findPointsCrossPolygons(List<Line> pg1, List<Line> pg2)
        {
            Dictionary<string, List<Vector>> res = new Dictionary<string, List<Vector>>();
            res.Add("pg1", getCrossPoints(pg1, pg2));
            res.Add("pg2", getCrossPoints(pg2, pg1));
            return res;
        }

        private static List<Vector> getCrossPoints(List<Line> pg1, List<Line> pg2)
        {

            List<Vector> points = new List<Vector>();
            for (int i = 0; i < pg1.Count; i++)
            {
                List<Vector> range = new List<Vector>();
                for (int j = 0; j < pg2.Count; j++)
                {
                    int crossFlag = -2;
                    Vector cross = getCrossPoint(pg1[i], pg2[j], out crossFlag, true);
                    if (crossFlag == 2)
                    {
                        range.Add(cross);
                    }
                }
                range = sortPoints(new Vector(pg1[i].X1, pg1[i].Y1), new Vector(pg1[i].X2, pg1[i].Y2), range);
                points.Add(new Vector(pg1[i].X1, pg1[i].Y1));
                points.AddRange(range);
            }
            return points;
        }

        private static List<Vector> sortPoints(Vector start, Vector end, List<Vector> points)
        {
            List<Vector> res = new List<Vector>();
            points.Sort();
            if (start.x > end.x)
            {
                points.Reverse();
            }
            res.AddRange(points);
            return res;
        }

        private static List<Line> testSidesLine(List<Line> lines1, List<Line> lines2)
        {
            for (int i = 0; i < lines1.Count; i++)
            {
                Vector centerPoint = getCenterPoint(lines1[i]);
                if (testRay(centerPoint, lines2))
                {
                    lines1[i].Tag = true;
                }
                else
                {
                    lines1[i].Tag = false;
                }
            }
            return lines1;
        }

        private static Vector getCenterPoint(Line line)
        {
            Vector res = new Vector();
            res.x = (line.X1 + line.X2) / 2;
            res.y = (line.Y1 + line.Y2) / 2;
            return res;
        }

        private static bool testRay(Vector p, List<Line> lines)
        {
            Line ray = getLine(p, new Vector(-200, -200));
            int count = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                int flag = -2;
                Vector cross = getCrossPoint(ray, lines[i], out flag, false);
                if (flag == 2) { count++; }
            }
            return (count % 2 == 1);
        }

        private static bool getCountPoint(Vector p, List<Line> lines)
        {
            int count = 0;
            foreach (Line l in lines)
            {
                p.x = Math.Round(p.x, 0);
                p.y = Math.Round(p.y, 0);
                l.X1 = Math.Round(l.X1, 0);
                l.X2 = Math.Round(l.X2, 0);
                l.Y1 = Math.Round(l.Y1, 0);
                l.Y2 = Math.Round(l.Y2, 0);
                if (((p.x == l.X1) && (p.y == l.Y1)) || ((p.x == l.X2) && (p.y == l.Y2))) { count++; }
            }
            return (count > 0);
        }

        private static List<Line> deleteFlag(bool flag, List<Line> lines)
        {
            List<Line> res = new List<Line>();
            foreach (Line line in lines)
            {
                if ((bool)line.Tag != flag)  // удаляем если линия помечена флагом
                {
                    res.Add(line);
                }
            }
            return res;
        }

        private static void unionPolygon()
        {
            //1) + Найти все точки пересечения между ребрами полигонов А и В;
            //2) + Добавить их в качестве новых вершин в оба полигона А и В;
            //3) + Разметить полигоны А и В: каждое ребро из А пометить флагом I(inside), если оно внутри полигона В, и O(outside), если оно снаружи. Аналогично для полигона В.
            //4) Теперь в зависимости от вида булевой операции:
            //а) объединение: удалить из А и В все ребра помеченные как I;
            //б) пересечение: удалить из А и В все ребра помеченные как O;
            //в) вычитание (А-В): удалить из А все I, а из В все O.
            //5) Слить то что осталось от А и В в один результирующий полигон.
            if (polygons.Count == 2)
            {
                Dictionary<string, List<Vector>> pointsPoligons = findPointsCrossPolygons(getLinesPolygon(polygons[0]), getLinesPolygon(polygons[1]));
                Polygon newPG1 = new Polygon();
                restorePolygonPoints(pointsPoligons["pg1"], newPG1);
                Polygon newPG2 = new Polygon();
                restorePolygonPoints(pointsPoligons["pg2"], newPG2);
                List<Line> lines1 = getLinesPolygon(newPG1);
                List<Line> lines2 = getLinesPolygon(newPG2);
                lines1 = testSidesLine(lines1, lines2);  // 
                lines2 = testSidesLine(lines2, lines1);
                
                //lines1 = deleteFlag(true, lines1);  // объединение
                //lines2 = deleteFlag(true, lines2);  //  
                lines1 = deleteFlag(false, lines1);  // пересечение
                lines2 = deleteFlag(false, lines2);  // 
                unionLines.Clear();
                unionLines.AddRange(lines1);
                unionLines.AddRange(lines2);
                for (int i = 0; i < unionLines.Count; i++)
                {
                    unionLines[i].StrokeThickness = 5;
                    unionLines[i].Stroke = new SolidColorBrush(Color.FromArgb(127, 0, 255, 0));
                }
            }
        }
    }
}
