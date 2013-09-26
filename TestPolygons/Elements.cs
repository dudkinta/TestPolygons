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
        public static ObservableCollection<Canvas> unionPolygons = new ObservableCollection<Canvas>();  // коллекция объединенных полигонов для маленьких картинок
        public static Dictionary<int, List<Polygon>> polyPolygons = new Dictionary<int, List<Polygon>>(); // коллекция объединенных полигонов
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
                    line.Points.Add(p);
                }
                line.Points.Add(p);
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
                    int polyCount = polygons.Count;
                    polygon.Tag = -1;
                    prepareUnionPolygon(polyCount - 1);
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
                List<VLine> lines = VGeometry.getLinesPolygon(polygons[i]);
                for (int j = 0; j < lines.Count; j++)
                {
                    double len = VGeometry.getDistance(lines[j], p);
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
                polygons[polygonId].Points.Insert(lineId + 1, p); // добавляем точку
                if (testListLines(polygons[polygonId]))  // если пересечения восстанавливаем точки
                {
                    restorePolygonPoints(points, polygons[polygonId]);
                    return false;
                }
                sendToCollect(polygonId);
                deletePair(polygonId);
                prepareUnionPolygon(polygonId);
            }
            return true;
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

        public static bool deletePolygonPoint(int polygonId, int pId) // фунция удаления точки полигона с проверкой самопересечений
        {
            int polyCount = polygons.Count;
            if (polygons[polygonId].Points.Count == 3)
            {
                polygons.RemoveAt(polygonId);
                plgns.RemoveAt(polygonId);
                deletePair(polygonId);
                return true;
            }
            else
            {
                List<Vector> points = savePolygonPoints(polygons[polygonId]);
                polygons[polygonId].Points.RemoveAt(pId);  // удаляем точку
                if (testListLines(polygons[polygonId]))  // если пересечения восстанавливаем точки
                {
                    restorePolygonPoints(points, polygons[polygonId]);
                    return false;
                }
                deletePair(polygonId);
                sendToCollect(polygonId);
                prepareUnionPolygon(polygonId);
                currentPoint.Stroke = Brushes.Transparent;
            }
            return true;
        }

        private static void deletePair(int polygonId)  // функция поиска и удаления объединенного полигона из словаря
        {
            if (polyPolygons.ContainsKey(polygonId))
            {
                polyPolygons.Remove(polygonId);
            }
            for (int i = 0; i < polygons.Count; i++)
            {
                if ((int)polygons[i].Tag == polygonId)
                {
                    if (polyPolygons.ContainsKey((int)polygons[i].Tag))
                    {
                        polyPolygons.Remove((int)polygons[i].Tag);
                        polygons[(int)polygons[i].Tag].Tag = -1;
                    }
                }
            }
            polygons[polygonId].Tag = -1;
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
                line.Points.Add(p);
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
                line.Points.Add(p1);
                line.StrokeThickness = 2;
            }
            return res;
        }

        public static void movePolygonPoint(Vector p, int polygonId, int centerId)  // функция проверки возможности перемещения точки полигона
        {
            List<Vector> points = savePolygonPoints(polygons[polygonId]);
            polygons[polygonId].Points[centerId] = p;  // перемещаем точку
            if (testListLines(polygons[polygonId]))  // если пересечения восстанавливаем точки
            {
                restorePolygonPoints(points, polygons[polygonId]);
            }
            else
            {
                sendToCollect(polygonId);
                deletePair(polygonId);
                currentPoint = setEllipseProperty(currentPoint, p, Brushes.Red, 5); // рисуем курсорчик
            }
            prepareUnionPolygon(polygonId);

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
                pg.Points.Add(points[i]);
            }
            return pg;
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
            List<VLine> lines = VGeometry.getLinesPolygon(pg);  // разбиваем на линии и проверяем пересечение каждой линии со всеми другими в полигоне
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
                        if ((cross - c).Lenght < (r - c).Lenght)
                        {
                            r = cross;
                        }
                    }
                }
            }
            return r;
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
            el.Width = el.Height = radius * 2;
            el.StrokeThickness = 5;
            el.Stroke = color;
            el.Margin = new Thickness(p.x - radius, p.y - radius, 0, 0);
            return el;
        }
        #endregion

        #region Подготовка и пересечение двух полигонов
        private static void prepareUnionPolygon(int polygonId)  // Функция реализующая алгоритм расчета продукта объединения/пересечения двух полигонов
        {
            //1) + Найти все точки пересечения между ребрами полигонов А и В;
            //2) + Добавить их в качестве новых вершин в оба полигона А и В;
            //3) + Разметить полигоны А и В: каждое ребро из А пометить флагом I(inside), если оно внутри полигона В, и O(outside), если оно снаружи. Аналогично для полигона В.
            //4) + Теперь в зависимости от вида булевой операции:
            //а) + объединение: удалить из А и В все ребра помеченные как I;
            //б) + пересечение: удалить из А и В все ребра помеченные как O;
            //5) + Слить то что осталось от А и В в один результирующий полигон.
            Polygon PG1 = polygons[polygonId];
            int pg2index = VGeometry.findPairPolygon(polygonId, polygons);
            if (pg2index != -1)
            {
                Polygon PG2 = polygons[pg2index];
                polygons[pg2index].Tag = polygonId;
                polygons[polygonId].Tag = pg2index;
                Dictionary<string, List<Vector>> pointsPoligons = VGeometry.findPointsCrossPolygons(VGeometry.getLinesPolygon(PG1), VGeometry.getLinesPolygon(PG2));
                Polygon newPG1 = restorePolygonPoints(pointsPoligons["pg1"], new Polygon());
                Polygon newPG2 = restorePolygonPoints(pointsPoligons["pg2"], new Polygon());
                List<VLine> lines1 = VGeometry.getLinesPolygon(newPG1);
                List<VLine> lines2 = VGeometry.getLinesPolygon(newPG2);
                lines1 = VGeometry.testSidesLine(lines1, lines2);
                lines2 = VGeometry.testSidesLine(lines2, lines1);
                lines1 = VGeometry.deleteFlag(true, lines1);  // объединение
                lines2 = VGeometry.deleteFlag(true, lines2);  //  
                //lines1 = deleteFlag(false, lines1);  // пересечение
                //lines2 = deleteFlag(false, lines2);  //
                List<VLine> unionLines = new List<VLine>();
                unionLines.AddRange(lines1);
                unionLines.AddRange(lines2);
                for (int i = 0; i < unionLines.Count; i++)
                {
                    unionLines[i].Stroke = Brushes.Tan;
                    unionLines[i].StrokeThickness = 2;
                }
                if (!polyPolygons.ContainsKey(polygonId))
                {
                    if (!polyPolygons.ContainsKey(pg2index))
                        polyPolygons.Add(pg2index, getPolyPolygon(unionLines));
                    else
                    {
                        polyPolygons[pg2index] = getPolyPolygon(unionLines);
                    }
                }
                else
                {
                    polyPolygons[polygonId] = getPolyPolygon(unionLines);
                }
            }
        }

        private static List<Polygon> getPolyPolygon(List<VLine> lines)
        {
            List<Polygon> res = new List<Polygon>();
            while (lines.Count > 0)
            {
                Polygon onePoly = new Polygon();
                PointCollection pColl = new PointCollection();
                Vector pIn = lines[0].Start;
                bool flag = true;
                pColl.Add(pIn);
                lines.RemoveAt(0);
                Vector nextPoint = pIn;
                while (flag)
                {
                    VLine line = new VLine();
                    lines = VGeometry.getNextLine(nextPoint, lines, out line);
                    if (line != null)
                    {
                        double lenA = (nextPoint - line.Start).Lenght;
                        double lenB = (nextPoint - line.End).Lenght;
                        if (lenA < lenB) { nextPoint = line.End; }
                        if (lenA >= lenB) { nextPoint = line.Start; }
                        if (pIn == nextPoint) { flag = false; }
                        else { pColl.Add(nextPoint); }
                    }
                    else { flag = false; }
                }
                onePoly.Points = pColl;
                onePoly.Fill = new SolidColorBrush(Color.FromArgb(127, 0, 255, 0));
                onePoly.StrokeThickness = 1;
                onePoly.Stroke = Brushes.Black;
                res.Add(onePoly);
            }
            return res;
        }  // Функция построения полигонов из разрозненных линий. (Финальный результат задания)
        #endregion
    }
}
