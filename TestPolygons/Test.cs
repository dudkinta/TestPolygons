using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestPolygons
{
    class Test
    {
        public void runTest()
        {
            VLine l1 = new VLine(new Vector(10,10), new Vector(10,100));
            VLine l2 = new VLine(new Vector(0, 90), new Vector(90, 90));
            if (VGeometry.crossPoint(l1, l2))
            {
                Vector cross = VGeometry.crossProduct;
            }
        }
    }
}
