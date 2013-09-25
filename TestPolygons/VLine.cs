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
        public VLine()
        {
            Start = new Vector();
            End = new Vector();
        }
        public VLine(Vector st, Vector en)
        {
            Start = st;
            End = en;
        }

        LineGeometry lineGeometry = new LineGeometry();

        public static readonly DependencyProperty StartPointProperty =
            LineGeometry.StartPointProperty.AddOwner(
                typeof(VLine),
                new FrameworkPropertyMetadata(new Point(0, 0),
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty EndPointProperty =
            LineGeometry.EndPointProperty.AddOwner(
                typeof(VLine),
                new FrameworkPropertyMetadata(new Point(0, 0),
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        private Point StartPoint
        {
            set { SetValue(StartPointProperty, value); }
            get { return (Point)GetValue(StartPointProperty); }
        }

        private Point EndPoint
        {
            set { SetValue(EndPointProperty, value); }
            get { return (Point)GetValue(EndPointProperty); }
        }

        public Vector Start
        {
            set
            {
                StartPoint = value.getPoint;
            }
            get
            {
                return new Vector(StartPoint);
            }
        }

        public Vector End
        {
            set
            {
                EndPoint = value.getPoint;
            }
            get
            {
                return new Vector(EndPoint);
            }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                lineGeometry.StartPoint = StartPoint;
                lineGeometry.EndPoint = EndPoint;
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
