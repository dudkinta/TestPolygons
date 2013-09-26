using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace TestPolygons
{
    class VPolyLine
    {
        public List<Vector> Points = new List<Vector>();
        GeometryGroup polyLineGeometry = new GeometryGroup();
        public static readonly DependencyProperty ChildrenProperty =
           GeometryGroup.ChildrenProperty.AddOwner(typeof(VPolyLine),
               new FrameworkPropertyMetadata(new PointCollection(),
                   FrameworkPropertyMetadataOptions.AffectsMeasure));

    }
}
