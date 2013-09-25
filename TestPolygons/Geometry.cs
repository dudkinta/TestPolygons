using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestPolygons
{
    static class VGeometry
    {
        public static Vector crossProduct = new Vector();

        public static bool crossPoint(VLine AB, VLine CD)
        {
            //http://fadedead.org/museum/vector-intersection-formula
            Vector result = new Vector();
            Vector p0 = AB.Start;
            Vector p1 = AB.End;
            Vector p2 = CD.Start;
            Vector p3 = CD.End;
            double z1 = p1.x - p0.x;
            double z2 = p3.x - p2.x;
            double w1 = p1.y - p0.y;
            double w2 = p3.y - p2.y;
            double k2 = (z1 * (p2.y - p0.y) + w1 * (p0.x - p2.x)) / (w1 * z2 - z1 * w2);
            result.x = p2.x + z2 * k2;
            result.y = p2.y + w2 * k2;
            if ((k2 > 0)&&(k2 < 1))
            {
                if (((result.x > AB.GetMinX) && (result.x < AB.GetMaxX)) && ((result.y > AB.GetMinY) && (result.y < AB.GetMaxY)))
                {
                    crossProduct = result;
                    return true;
                }
            }
            return false;
        }
    }
}
