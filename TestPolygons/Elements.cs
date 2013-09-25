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
        #region Объявление переменных
        public static string debug = "";  // для вывода информации в lbHint на StatusBar главного окна
        public static Ellipse currentPoint = new Ellipse();  // маркер выбраной точки
        public static List<Polygon> polygons = new List<Polygon>();  // коллекция полигонов
        public static ObservableCollection<Canvas> plgns = new ObservableCollection<Canvas>();  // коллекция полигонов для маленьких картинок
        public static List<Polygon> polyPolygon = new List<Polygon>();
        public static List<VLine> unionLines = new List<VLine>();  // полигон объединенный в виде массива линиц
        public static Polyline line = new Polyline(); // недорисованный полигон
        #endregion

        #region Добавление элементов
        public static void addPoint(Vector p) // функци добавления сегмента к недостроенному полигону
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
                    line.Stroke = Brushes.Black;
                    line.StrokeThickness = 2;
                    line.Points.Add(p.getPoint);
                }
                line.Points.Add(p.getPoint);
            }
            if (ptId == 0)
            {
                addPolygon();
            }
            line.StrokeThickness = 2;
        }

        public static bool addPolygon() // фунция конвертирования недостроенного полигона в построенный и добавление его в коллекцию
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

        public static void addToCollect(Polygon pg) // функция создания и добавления в коллекцию миниатюр
        {
            List<Vector> points = normalizePoints(savePolygonPoints(pg), 130, 130); //нормализуем точки полигона для того что бы он влез в миниатюру без искажений
            Polygon newPG = new Polygon();
            restorePolygonPoints(points, newPG); // восстанавливаем нормализованные точки в новый полигон
            newPG = setPolygonProperty(newPG);
            Canvas cnv = new Canvas();
            cnv.Children.Add(newPG);
            plgns.Add(cnv); // и добавляем в коллекцию
        }

        private static void sendToCollect(int pg) // функция обновления полигона в коллекции миниатюр
        {
            List<Vector> points = normalizePoints(savePolygonPoints(polygons[pg]), 130, 130);
            Polygon newPG = new Polygon();
            restorePolygonPoints(points, newPG);
            newPG = setPolygonProperty(newPG);
            plgns[pg].Children.Clear();
            plgns[pg].Children.Add(newPG);
        }

        public static bool addPointPolygon(Vector p) // функция добавление точки в построенный полигон с проверкой самопересечения
        {
            double minLenght = double.MaxValue;
            int polygonId = -1;
            int lineId = -1;
            for (int i = 0; i < polygons.Count; i++)
            {
                List<VLine> lines = getLinesPolygon(polygons[i]);
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
                polygons[polygonId].Points.Insert(lineId + 1, p.getPoint); // добавляем точку
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
        #endregion

        #region Удаление элементов
        public static void deleteLastPoint()  // функция удаления последней точки недостроенного полигона
        {
            int pCount = line.Points.Count;
            if (pCount > 0)
            {
                line.Points.RemoveAt(pCount - 1);
            }
        }

        public static bool deletePolygonPoint(int pg, int pId) // фунция удаления точки полигона с проверкой самопересечений
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
        #endregion

        #region Модификация элементов
        public static bool moveLastSegment(Vector p)  // функция проверки возможности перемещать последний сегмент недостроенного полигона
        {
            bool res = false;
            int pCount = line.Points.Count;
            if (pCount >= 1)
            {
                Vector testPoint = new Vector(line.Points[pCount - 1]);
                line.Points.RemoveAt(pCount - 1);
                line.Points.Add(p.getPoint);
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
                line.Points.Add(p1.getPoint);
                line.StrokeThickness = 2;
            }
            return res;
        }

        public static void movePolygonPoint(Vector p, int polygonId, int centerId)  // функция проверки возможности перемещения точки полигона
        {
            List<Vector> points = savePolygonPoints(polygons[polygonId]);
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

        private static List<Vector> normalizePoints(List<Vector> points, int normX, int normY) // нормализация точек полигона для вписывания в миниатюру
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
        #endregion

        #region Конвертация объектов в линии и точки
        public static List<Vector> savePolygonPoints(Polygon pg)  // функция конвертирования полигона в коллекцию точек
        {
            List<Vector> points = new List<Vector>(); // список точек полигона
            for (int i = 0; i < pg.Points.Count; i++)  // сохраняем точки для восстановления в случае необходимости
            {
                points.Add(new Vector(pg.Points[i]));
            }
            return points;
        }

        public static Polygon restorePolygonPoints(List<Vector> points, Polygon pg) // восстанавливаем точки полигона из коллекции
        {
            pg.Points.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                pg.Points.Add(points[i].getPoint);
            }
            return pg;
        }

        private static List<VLine> getLinesPolygon(Polygon pg) // функция конвертирования полигона в коллекцию линий
        {
            List<VLine> lines = new List<VLine>();  // список линий полигона
            for (int i = 0; i < pg.Points.Count; i++) // составляем список линий полигона
            {
                int leftId = (i == pg.Points.Count - 1) ? 0 : i + 1;
                VLine leftLine = new VLine(new Vector(pg.Points[i]), new Vector(pg.Points[leftId]));  // левая линия от точки
                lines.Add(leftLine);
            }
            return lines;
        }
        #endregion

        #region Поисковые и проверочные функции
        public static Vector getPoint(Vector p, out int polygon, out int pNum, int radius, bool isPolygon) // поиск объекта к которому относится точка на экране
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
        
        private static bool testListLines(Polygon pg)  // вспомогательная функция для проверки самопересечений полигона (добавление, удаление и перемещение вершины полигона)
        {
            List<VLine> lines = getLinesPolygon(pg);  // разбиваем на линии и проверяем пересечение каждой линии со всеми другими в полигоне
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = 0; j < lines.Count; j++)
                {
                    if (VGeometry.crossPoint(lines[i], lines[j])) { return true; }
                }
            }
            return false;
        }

        private static Vector testCrossLine(Vector r)  // тестирование последнего сегмента на самоперечесение
        {
            Vector res = new Vector(r);
            int pCount = line.Points.Count;
            if (pCount > 1)
            {
                for (int i = 0; i < pCount - 1; i++)
                {
                    Vector a = new Vector(line.Points[i]), b = new Vector(line.Points[i + 1]);
                    Vector c = new Vector(line.Points[pCount - 2]), d = new Vector(line.Points[pCount - 1]);
                    VLine l1 = new VLine(a, b), l2 = new VLine(c, d);
                    if (VGeometry.crossPoint(l1, l2))
                    {
                        Vector cross = VGeometry.crossProduct;
                        if ((cross - c).Lenght < (res - c).Lenght)
                        {
                            res = cross;
                        }
                    }
                }
            }
            return res;
        }

        private static double getLenght(VLine line, Vector c) // вычисление расстояние от точки до отрезка
        {
            Vector a = line.Start;
            Vector b = line.End;
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
        #endregion

        #region Установка свойств "по-умолчанию"
        public static Polygon setPolygonProperty(Polygon pg)  // установка раскраски полигона "по умолчанию"
        {
            pg.Fill = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127));
            pg.StrokeThickness = 1;
            pg.Stroke = Brushes.Blue;
            return pg;
        }

        private static Ellipse setEllipseProperty(Ellipse el, Vector p, Brush color, int radius) // установка раскраски эллипса "по умолчанию"
        {
            el.Width = radius * 2;
            el.Height = radius * 2;
            el.StrokeThickness = 5;
            el.Stroke = color;
            el.Margin = new Thickness(p.x - radius, p.y - radius, 0, 0);
            return el;
        }
        #endregion



        #region Подготовка коллекции линий продукта пересения двух полигонов
        private static void unionPolygon()  // Функция реализующая алгоритм расчета продукта объединения/пересечения двух полигонов
        {
            //1) + Найти все точки пересечения между ребрами полигонов А и В;
            //2) + Добавить их в качестве новых вершин в оба полигона А и В;
            //3) + Разметить полигоны А и В: каждое ребро из А пометить флагом I(inside), если оно внутри полигона В, и O(outside), если оно снаружи. Аналогично для полигона В.
            //4) + Теперь в зависимости от вида булевой операции:
            //а) + объединение: удалить из А и В все ребра помеченные как I;
            //б) + пересечение: удалить из А и В все ребра помеченные как O;
            //в) + вычитание (А-В): удалить из А все I, а из В все O.
            //5) Слить то что осталось от А и В в один результирующий полигон.
            if (polygons.Count == 2)
            {
                Dictionary<string, List<Vector>> pointsPoligons = findPointsCrossPolygons(getLinesPolygon(polygons[0]), getLinesPolygon(polygons[1]));
                Polygon newPG1 = restorePolygonPoints(pointsPoligons["pg1"], new Polygon());
                Polygon newPG2 = restorePolygonPoints(pointsPoligons["pg2"], new Polygon());
                List<VLine> lines1 = getLinesPolygon(newPG1);
                List<VLine> lines2 = getLinesPolygon(newPG2);
                lines1 = testSidesLine(lines1, lines2);   
                lines2 = testSidesLine(lines2, lines1);
                lines1 = deleteFlag(true, lines1);  // объединение
                lines2 = deleteFlag(true, lines2);  //  
                //lines1 = deleteFlag(false, lines1);  // пересечение
                //lines2 = deleteFlag(false, lines2);  //
                unionLines.Clear();
                unionLines.AddRange(lines1);
                unionLines.AddRange(lines2);
                for (int i = 0; i < unionLines.Count; i++)
                {
                    unionLines[i].Stroke = Brushes.Tan;
                    unionLines[i].StrokeThickness = 2;
                }
                polyPolygon.Clear();
                polyPolygon.Add(getPolyPolygon(unionLines));
            }
        }

        private static Dictionary<string, List<Vector>> findPointsCrossPolygons(List<VLine> pg1, List<VLine> pg2)  // конвертация полигонов в наборы точек
        {
            Dictionary<string, List<Vector>> res = new Dictionary<string, List<Vector>>();
            res.Add("pg1", getCrossPoints(pg1, pg2));
            res.Add("pg2", getCrossPoints(pg2, pg1));
            return res;
        }

        private static List<Vector> getCrossPoints(List<VLine> pg1, List<VLine> pg2) // нахождение, добавление и сортировка тчек пересечения полигонов
        {
            List<Vector> points = new List<Vector>();
            for (int i = 0; i < pg1.Count; i++)
            {
                List<Vector> range = new List<Vector>();
                for (int j = 0; j < pg2.Count; j++)
                {
                    if (VGeometry.crossPoint(pg1[i], pg2[j]))
                    {
                        range.Add(VGeometry.crossProduct);
                    }
                }
                range.Sort();
                if (pg1[i].Start.x > pg1[i].End.x)
                {
                    range.Reverse();
                }
                points.Add(pg1[i].Start);
                points.AddRange(range);
            }
            return points;
        }

        private static List<VLine> testSidesLine(List<VLine> lines1, List<VLine> lines2) // тестирование средних точек каждой линии на предмет внутри или снаружи чужого полигона
        {
            for (int i = 0; i < lines1.Count; i++)
            {
                Vector centerPoint = lines1[i].centerPoint;
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

        private static bool testRay(Vector p, List<VLine> lines) // фугкция тестирования точки (внутри/снаружи) полигона методом луча (true - внутри,false - снаружи)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            foreach (VLine l in lines)
            {
                minX = (Math.Min(l.Start.x, l.End.x) < minX) ? Math.Min(l.Start.x, l.End.x) : minX;
                minY = (Math.Min(l.Start.y, l.End.y) < minY) ? Math.Min(l.Start.y, l.End.y) : minY;
            }
            VLine ray = new VLine(p, new Vector(minX - 1, minY - 1));
            int count = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                if (VGeometry.crossPoint(ray, lines[i]))
                {
                    count++;
                }
            }
            return (count % 2 == 1);
        }

        private static List<VLine> deleteFlag(bool flag, List<VLine> lines) // функция удаления линий с пометками (внутри/снаружи)
        {
            List<VLine> res = new List<VLine>();
            foreach (VLine line in lines)
            {
                if ((bool)line.Tag != flag)  // удаляем если линия помечена флагом
                {
                    res.Add(line);
                }
            }
            return res;
        }
        #endregion










        



        
        private static Polygon getPolyPolygon(List<VLine> lines)
        {
            Polygon res = new Polygon();
            PointCollection pColl = new PointCollection();
            Vector pIn = lines[0].Start;
            bool flag = true;
            pColl.Add(new Point(pIn.x, pIn.y));
            Vector nextPoint = pIn;
            while (flag)
            {
                VLine line = getNearLine(pIn, lines);
                if (line != null)
                {
                    double lenA = (nextPoint - line.Start).Lenght;
                    double lenB = (nextPoint - line.End).Lenght;
                    if (lenA < lenB) { nextPoint = line.End; }
                    if (lenA >= lenB) { nextPoint = line.Start; }
                    if (nextPoint == pIn) { flag = false; }
                    else { pColl.Add(new Point(nextPoint.x, nextPoint.y));}
                }
            } 
            
            res.Points = pColl;
            res.Fill = new SolidColorBrush(Color.FromArgb(127, 0, 255, 0));
            res.StrokeThickness = 1;
            res.Stroke = Brushes.Black;
            return res;
        }

        private static VLine getNearLine(Vector pIn, List<VLine> lines)
        {
            VLine res = null;
            double minL = double.MaxValue;
            foreach (VLine line in lines)
            {
                double lenA = (pIn - line.Start).Lenght;
                double lenB = (pIn - line.End).Lenght;
                if (Math.Min(lenA, lenB) < minL)
                {
                    minL = Math.Min(lenA, lenB);
                    res = line;
                }
            }
            return res;
        }
    }
}
