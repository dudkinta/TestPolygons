using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TestPolygons
{
    class VPolyLine : Shape
    {
        private List<Vector> _Dots = new List<Vector>();
        private GeometryGroup polyLineGeometry = new GeometryGroup();
        private List<LineGeometry> lines = new List<LineGeometry>();
        public static readonly DependencyProperty DotsProperty;
        static VPolyLine()
        {
            DotsProperty = DependencyProperty.Register(
            "Dots",
            typeof(List<Vector>),
            typeof(VPolyLine),
            new FrameworkPropertyMetadata(new List<Vector>()));
        }
        public PointCollection Points
        {
            get
            {
                PointCollection ps = new PointCollection();
                foreach (Vector p in _Dots)
                {
                    ps.Add(p.getPoint);
                }
                return ps;
            }
        }
        public void Add(Vector p)
        {
            _Dots.Add(p);
            if (lines.Count != 0)
            {
                lines.Add(new LineGeometry(_Dots.Last().getPoint, p.getPoint));
            }
        }

        public void RemoveLast()
        {
            _Dots.Remove(_Dots.Last());
            if (lines.Count != 0)
            {
                lines.Remove(lines.Last());
            }
        }
        public void Clear()
        {
            _Dots.Clear();
            lines.Clear();
        }
        public int Count
        {
            get
            {
                return _Dots.Count;
            }
        }
        protected override Geometry DefiningGeometry
        {
            get
            {
                polyLineGeometry.Children.Clear();
                foreach (LineGeometry lGeom in lines)
                {
                    polyLineGeometry.Children.Add(lGeom);
                }
                return polyLineGeometry;
            }
        }



    }
}
