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
        public struct ScaleStruct  // структура для определения коэффициента маштабирования и смещения на миниатюре
        {
            public double minX;
            public double minY;
            public double sx;
            public double sy;
            public double scale;
        }
        public static string debug = "";
        public static List<VLine> debugLines = new List<VLine>();

        public static Ellipse currentPoint = new Ellipse();  // маркер выбраной точки
        public static List<Polygon> polygons = new List<Polygon>();  // коллекция полигонов
        public static ObservableCollection<Canvas> plgns = new ObservableCollection<Canvas>();  // коллекция полигонов для маленьких картинок
        public static ObservableCollection<Canvas> unionPolygons = new ObservableCollection<Canvas>();  // коллекция объединенных полигонов для маленьких картинок
        public static List<List<Polygon>> polyPolygons = new List<List<Polygon>>(); // коллекция объединенных полигонов
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
                    polygons.Add(polygon);
                    line = new Polyline();
                    res = true;
                    addToCollect(polygon);
                    int polyCount = polygons.Count;
                    prepareUnionPolygon();
                }
            }
            return res;
        }

        public static void addToCollect(Polygon pg) // функция создания и добавления в коллекцию миниатюр
        {
            List<Vector> points = savePolygonPoints(pg);
            ScaleStruct scaler = VGeometry.normalizePoints(points, 130, 130); //нормализуем точки полигона для того что бы он влез в миниатюру без искажений
            points = VGeometry.scalePoints(points, scaler);
            Polygon newPG = new Polygon();
            restorePolygonPoints(points, newPG); // восстанавливаем нормализованные точки в новый полигон
            newPG.Fill = Brushes.Gray;
            newPG.Stroke = Brushes.Black;
            newPG.StrokeThickness = 1;
            Canvas cnv = new Canvas();
            cnv.Children.Add(newPG);
            plgns.Add(cnv); // и добавляем в коллекцию
        }
        public static void addToUnionCollect(List<List<Polygon>> uPgs) // функция создания и добавления в коллекцию миниатюр
        {
            foreach (List<Polygon> pgs in uPgs)
            {
                Canvas cnv = new Canvas();
                ScaleStruct scaler = VGeometry.normalizePoints(savePolygonPoints(pgs[0]), 130, 130); //нормализуем точки полигона для того что бы он влез в миниатюру без искажений
                for (int i = 0; i < pgs.Count; i++)
                {
                    List<Vector> points = savePolygonPoints(pgs[i]);
                    points = VGeometry.scalePoints(points, scaler);
                    Polygon newPG = new Polygon();
                    restorePolygonPoints(points, newPG); // восстанавливаем нормализованные точки в новый полигон
                    if (i == 0)
                    {
                        newPG.Fill = Brushes.Green;
                    }
                    else
                    {
                        newPG.Fill = Brushes.White;
                    }
                    newPG.Stroke = Brushes.Black;
                    newPG.StrokeThickness = 1;
                    cnv.Children.Add(newPG);
                    
                }
                unionPolygons.Add(cnv); // и добавляем в коллекцию
            }
        }

        private static void sendToCollect(int pg) // функция обновления полигона в коллекции миниатюр
        {
            List<Vector> points = savePolygonPoints(polygons[pg]);
            ScaleStruct scaler = VGeometry.normalizePoints(points, 130, 130); //нормализуем точки полигона для того что бы он влез в миниатюру без искажений
            points = VGeometry.scalePoints(points, scaler);
            Polygon newPG = new Polygon();
            restorePolygonPoints(points, newPG);
            newPG.Fill = Brushes.Gray;
            newPG.Stroke = Brushes.Black;
            newPG.StrokeThickness = 1;
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
                prepareUnionPolygon();
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
                prepareUnionPolygon();
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
                sendToCollect(polygonId);
                prepareUnionPolygon();
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
                currentPoint = setEllipseProperty(currentPoint, p, Brushes.Red, 5); // рисуем курсорчик
            }
            prepareUnionPolygon();

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

        private static int findPairPolygon(int polygonId)  // поиск пересекаемого полигона 
        {
            List<VLine> linesPG1 = VGeometry.getLinesPolygon(polygons[polygonId]);
            for (int i = 0; i < polygons.Count; i++)
            {
                if (i != polygonId)
                {
                    if ((int)polygons[i].Tag == -1)
                    {
                        List<VLine> linesPG2 = VGeometry.getLinesPolygon(polygons[i]);
                        List<Vector> pList = VGeometry.getCrossPoints(linesPG1, linesPG2);
                        if ((pList.Count - linesPG1.Count) != 0)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }
        #endregion

        #region Установка свойств "по-умолчанию"
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
        public static void prepareUnionPolygon()  // Функция реализующая алгоритм расчета продукта объединения/пересечения двух полигонов
        {
            //1) + Найти все точки пересечения между ребрами полигонов А и В;
            //2) + Добавить их в качестве новых вершин в оба полигона А и В;
            //3) + Разметить полигоны А и В: каждое ребро из А пометить флагом I(inside), если оно внутри полигона В, и O(outside), если оно снаружи. Аналогично для полигона В.
            //4) + Теперь в зависимости от вида булевой операции:
            //а) + объединение: удалить из А и В все ребра помеченные как I;
            //б) + пересечение: удалить из А и В все ребра помеченные как O;
            //5) + Слить то что осталось от А и В в один результирующий полигон.

            for (int i = 0; i < polygons.Count; i++)   // очистка коллекций
            {
                polygons[i].Tag = -1;
            }
            polyPolygons.Clear();
            unionPolygons.Clear();
            for (int i = 0; i < polygons.Count; i++)
            {
                if ((int)polygons[i].Tag == -1)
                {
                    Polygon PG1 = polygons[i];
                    int pg2index = findPairPolygon(i);
                    if (pg2index != -1)
                    {
                        Polygon PG2 = polygons[pg2index];
                        Dictionary<string, List<Vector>> pointsPoligons = VGeometry.findPointsCrossPolygons(VGeometry.getLinesPolygon(PG1), VGeometry.getLinesPolygon(PG2));
                        if ((pointsPoligons["pg1"].Count > 0) && (pointsPoligons["pg2"].Count > 0))
                        {
                            polygons[pg2index].Tag = i;
                            polygons[i].Tag = pg2index;
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
                            List<Polygon> polyList = getPolyPolygon(unionLines);
                            polyPolygons.Add(polyList);
                        }
                    }
                }
            }
            addToUnionCollect(polyPolygons);
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
                res.Add(onePoly);
            }
            return res;
        }  // Функция построения полигонов из разрозненных линий. (Финальный результат задания)
        #endregion
    }
}
