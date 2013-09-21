using System.Collections.Generic;
using System.Windows.Shapes;

namespace TestPolygons.Tests
{
    public static class Tests
    {
        private static List<Line> lines = new List<Line>();

        public static void InitLinesTest()
        {
            lines.Add(Elements.getLine(0, 0, 1, 1));
            lines.Add(Elements.getLine(2, 2, 3, 3));

            lines.Add(Elements.getLine(1, 1, 3, 2));
            lines.Add(Elements.getLine(2, 1, 1, 3));

            lines.Add(Elements.getLine(1, 1, 5, 5));
            lines.Add(Elements.getLine(2, 2, 4, 4));

            lines.Add(Elements.getLine(5, 5, 1, 1));
            lines.Add(Elements.getLine(2, 2, 4, 4));
        }
    }
}
