using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TestPolygons
{
    class VLine : Shape
    {
        LineGeometry lineGeometry = new LineGeometry();

        public static readonly DependencyProperty StartProperty;
        public static readonly DependencyProperty EndProperty;
        static VLine()
        {
            StartProperty = DependencyProperty.Register(
            "Start",
            typeof(Vector),
            typeof(VLine),
            new FrameworkPropertyMetadata(new Vector()));
            EndProperty = DependencyProperty.Register(
            "End",
            typeof(Vector),
            typeof(VLine),
            new FrameworkPropertyMetadata(new Vector()));
        }
        
        public VLine(Vector st, Vector en)
        {
            Start = st;
            End = en;
        }

        public Vector Start
        {
            get { return (Vector)base.GetValue(StartProperty); }
            set { base.SetValue(StartProperty, value); }
        }

        public Vector End
        {
            get { return (Vector)base.GetValue(EndProperty); }
            set { base.SetValue(EndProperty, value); }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                lineGeometry.StartPoint = Start.getPoint;
                lineGeometry.EndPoint = End.getPoint;
                return lineGeometry;
            }
        }

        public double GetMaxX
        {
            get
            {
                return (Start.x >= End.x) ? Start.x : End.x;
            }
        }
        public double GetMaxY
        {
            get
            {
                return (Start.y >= End.y) ? Start.y : End.y;
            }
        }
        public double GetMinX
        {
            get
            {
                return (Start.x < End.x) ? Start.x : End.x;
            }
        }
        public double GetMinY
        {
            get
            {
                return (Start.y < End.y) ? Start.y : End.y;
            }
        }

        public Vector centerPoint
        {
            get
            {
                return new Vector((Start.x + End.x) / 2, (Start.y + End.y) / 2);
            }
        }
    }
}
