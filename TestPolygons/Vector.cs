using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace TestPolygons
{
    public class Vector : IComparable<Vector>   // класс был написан для другого проекта поэтому много лишнего. Аналогичный класс в С# чем то непонравился, но чем именно уже не помню :)
    {
        public double x;
        public double y;

        public Vector()
        {
            this.x = 0;
            this.y = 0;
        }
        public Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector(Point p)
        {
            this.x = p.X;
            this.y = p.Y;
        }

        public Vector(Vector p)
        {
            this.x = p.x;
            this.y = p.y;
        }

        public int CompareTo(Vector b)
        {
            if (this.x < b.x)
                return -1;
            if (this.x > b.x)
                return 1;
            else
            {
                if (this.y < b.y)
                    return -1;
                if (this.y > b.y)
                    return 1;
            }
            return 0;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Vector p = obj as Vector;
            if ((System.Object)p == null)
            {
                return false;
            }
            return (this.x == p.x) && (this.y == p.y);
        }
        public override int GetHashCode()
        {
            return (int)this.x ^ (int)this.y;
        }
        public override string ToString()
        {
            return this.x.ToString() + ";" + this.y.ToString();
        }
        #region Операторы
        #region Сумма
        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.x + b.x, a.y + b.y);
        }
        public static Vector operator +(Vector a, int b)
        {
            return new Vector(a.x + b, a.y + b);
        }
        public static Vector operator +(int a, Vector b)
        {
            return new Vector(a + b.x, a + b.y);
        }
        #endregion
        #region Разность
        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.x - b.x, a.y - b.y);
        }
        public static Vector operator -(Vector a, int b)
        {
            return new Vector(a.x - b, a.y - b);
        }
        public static Vector operator -(int a, Vector b)
        {
            return new Vector(a - b.x, a - b.y);
        }
        #endregion
        #region Произведение
        public static Vector operator *(Vector a, double b)
        {
            return new Vector(a.x * b, a.y * b);
        }
        public static Vector operator *(double a, Vector b)
        {
            return new Vector(a * b.x, a * b.y);
        }
        public static double operator *(Vector a, Vector b)
        {
            return (a.x * b.x + a.y * b.y);
        }
        #endregion
        #region Деление
        public static Vector operator /(Vector a, double b)
        {
            double c = 1 / b;
            return a * c;
        }
        public static Vector operator /(double a, Vector b)
        {
            double c = 1 / a;
            return c * b;
        }
        public static double operator /(Vector a, Vector b)
        {
            Vector c = 1 / b;
            return a * c;
        }
        #endregion
        #region Равно
        public static bool operator ==(Vector a, Vector b)
        {
            return ((a.x == b.x) && (a.y == b.y));
        }
        #endregion
        #region Неравно
        public static bool operator !=(Vector a, Vector b)
        {
            return ((a.x != b.x) || (a.y != b.y));
        }
        #endregion
        public static implicit operator Vector(Point p)
        {
            return new Vector(p);
        }
        public static implicit operator Point(Vector v)
        {
            return v.getPoint;
        }
        #endregion
        #region Методы
        private double LenghtPow2
        {
            get
            {
                return Math.Pow(this.x, 2) + Math.Pow(this.y, 2);
            }
        }
        public double Lenght
        {
            get
            {
                return Math.Sqrt(this.LenghtPow2);
            }
        }
        public Vector Normal
        {
            get
            {
                double len = this.Lenght;
                return new Vector(this.x / len, this.y / len);
            }
        }
        public double Angle
        {
            get
            {
                return Math.Atan2(this.y, this.x) * 180 / Math.PI;
            }
        }
        public Vector Round(int i)
        {
            return new Vector(Math.Round(this.x, i), Math.Round(this.y, i));
        }
        private Point getPoint
        {
            get
            {
                return new Point(this.x, this.y);
            }
        }
        public bool isNan
        {
            get
            {
                return ((Double.IsNaN(x)) || (Double.IsNaN(y)));
            }
        }
        public bool isInfinity
        {
            get
            {
                return ((Double.IsInfinity(x)) || (Double.IsInfinity(y)));
            }
        }
        public bool isNull
        {
            get
            {
                return ((x==0) && (y==0));
            }
        }
        #endregion
    }
}
