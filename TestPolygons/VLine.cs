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
        protected PathGeometry pathgeo;
        protected PathFigure pathfigLine;
        protected PolyLineSegment polysegLine;

        public VLine()
        {
            pathgeo = new PathGeometry();
            pathfigLine = new PathFigure();
            polysegLine = new PolyLineSegment();
            pathfigLine.Segments.Add(polysegLine);
        }

        public VLine(Vector a, Vector b)
        {
            pathgeo = new PathGeometry();
            pathfigLine = new PathFigure();
            polysegLine = new PolyLineSegment();
            pathfigLine.Segments.Add(polysegLine);
            Start = a;
            End = b;
        }

        public static readonly DependencyProperty X1Property =
            Line.X1Property.AddOwner(typeof(VLine),
                new FrameworkPropertyMetadata(0.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty Y1Property =
            Line.Y1Property.AddOwner(typeof(VLine),
                new FrameworkPropertyMetadata(0.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty X2Property =
            Line.X2Property.AddOwner(typeof(VLine),
                new FrameworkPropertyMetadata(0.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty Y2Property =
            Line.Y2Property.AddOwner(typeof(VLine),
                new FrameworkPropertyMetadata(0.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        private double X1
        {
            set { SetValue(X1Property, value); }
            get { return (double)GetValue(X1Property); }
        }
        private double Y1
        {
            set { SetValue(Y1Property, value); }
            get { return (double)GetValue(Y1Property); }
        }
        private double X2
        {
            set { SetValue(X2Property, value); }
            get { return (double)GetValue(X2Property); }
        }
        private double Y2
        {
            set { SetValue(Y2Property, value); }
            get { return (double)GetValue(Y2Property); }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                pathgeo.Figures.Clear();
                pathfigLine.StartPoint = Start;
                polysegLine.Points.Clear();
                polysegLine.Points.Add(End);
                pathgeo.Figures.Add(pathfigLine);
                return pathgeo;
            }
        }

        public Vector Start
        {
            get
            {
                return new Vector(X1, Y1);
            }
            set
            {
                X1 = value.x;
                Y1 = value.y;
            }

        }
        public Vector End
        {
            get
            {
                return new Vector(X2, Y2);
            }
            set
            {
                X2 = value.x;
                Y2 = value.y;
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
        public double getWidth
        {
            get
            {
                return End.x - Start.x;
            }
        }
        public double getHeight
        {
            get
            {
                return End.y - Start.y;
            }
        }
        public Vector centerPoint
        {
            get
            {
                return new Vector((Start.x + End.x) / 2, (Start.y + End.y) / 2);
            }
        }
        public double getLenght
        {
            get
            {
                return (End - Start).Lenght;
            }
        }
    }
}
