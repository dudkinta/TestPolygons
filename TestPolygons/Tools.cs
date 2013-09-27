using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPolygons
{
    static class Tools  // класс инструментов  ("стрелочка" и "полигон")
    {
        public enum ToolType : int
        {
            arrow = 0,
            polygon = 1
        }

        public static ToolType type = ToolType.arrow;
    }
}
