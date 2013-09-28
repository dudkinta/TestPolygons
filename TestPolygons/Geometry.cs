using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;

namespace TestPolygons
{
    static class VGeometry
    {
        public static Vector crossProduct = new Vector(); // точка пересечения (заполняется в crossPoint) // для работы в разных потоках необходимо перенести в функцию crossPoint как out  

        public static bool crossPoint(VLine AB, VLine CD) // поиск точки пересечения между двумя отрезками
        {
            Vector result = new Vector();
            double b = (AB.getWidth * (CD.Start.y - AB.Start.y) + AB.getHeight * (AB.Start.x - CD.Start.x)) / (AB.getHeight * CD.getWidth - AB.getWidth * CD.getHeight);
            double a = (CD.Start.x + b * CD.getWidth - AB.Start.x) / AB.getWidth;
            result.x = CD.Start.x + CD.getWidth * b;
            result.y = CD.Start.y + CD.getHeight * b;
            if (((b > 0) && (b < 1)) && ((a > 0) && (a < 1)))
            {
                crossProduct = result;
                return true;
            }
            return false;
        }

        public static double getDistance(VLine line, Vector c) // вычисление расстояние от точки до отрезка
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

        public static List<VLine> getLinesPolygon(Polygon pg) // функция конвертирования полигона в коллекцию линий
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

        public static Dictionary<string, List<Vector>> findPointsCrossPolygons(List<VLine> pg1, List<VLine> pg2)  // сбор точек пересечения двух полигонов
        {
            Dictionary<string, List<Vector>> res = new Dictionary<string, List<Vector>>();
            res.Add("pg1", getCrossPoints(pg1, pg2));
            res.Add("pg2", getCrossPoints(pg2, pg1));
            return res;
        }

        public static List<Vector> getCrossPoints(List<VLine> pg1, List<VLine> pg2) // нахождение, добавление и сортировка точек пересечения полигонов
        {
            List<Vector> points = new List<Vector>();
            for (int i = 0; i < pg1.Count; i++)
            {
                List<Vector> range = new List<Vector>();
                for (int j = 0; j < pg2.Count; j++)
                {
                    if (crossPoint(pg1[i], pg2[j]))
                    {
                        range.Add(crossProduct);
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

        public static List<VLine> testSidesLine(List<VLine> lines1, List<VLine> lines2) // тестирование средних точек каждой линии на предмет внутри или снаружи чужого полигона
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

        public static bool testRay(Vector p, List<VLine> lines) // функция тестирования точки (внутри/снаружи) полигона методом луча (true - внутри,false - снаружи)
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
                if (crossPoint(ray, lines[i]))
                {
                    count++;
                }
            }
            return (count % 2 == 1);
        }

        public static List<VLine> deleteFlag(bool flag, List<VLine> lines) // функция удаления линий с пометками (внутри/снаружи)
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

        public static List<VLine> getNextLine(Vector pIn, List<VLine> lines, out VLine ln)  // функция поиска линии ближайшей к точке для построения ломаной кривой описывающей полигон
        {
            ln = null;
            double minL = double.MaxValue;
            foreach (VLine line in lines)
            {
                double len = VGeometry.getDistance(line, pIn);
                if (len < minL)
                {
                    minL = len;
                    ln = line;
                }
            }
            if (minL > 0.1)
            {
                ln = null;
                return lines;
            }
            lines.Remove(ln);
            return lines;
        }

        public static Elements.ScaleStruct normalizePoints(List<Vector> points, int normX, int normY) // нормализация точек полигона для вписывания в миниатюру
        {
            Elements.ScaleStruct res = new Elements.ScaleStruct();
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
            
            res.minX = minX; res.minY = minY;
            res.sx = sx; res.sy = sy;
            res.scale = scale;
            return res;
        }

        public static List<Vector> scalePoints(List<Vector> points, Elements.ScaleStruct scaler)  // маштабирование точек полигона
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].x = points[i].x - scaler.minX;
                points[i].x = (points[i].x * scaler.scale) + scaler.sx;
                points[i].y = points[i].y - scaler.minY;
                points[i].y = (points[i].y * scaler.scale) + scaler.sy;
            }
            return points;
        }
    }
}
