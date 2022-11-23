using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Core.Model
{
    public struct Point : IComparable
    {
        public static int CalculateDistance(Point p1, Point p2)
        {
            return (int)Math.Sqrt(
                (p1.Row - p2.Row) * (p1.Row - p2.Row) +
                (p2.Column - p1.Column) * (p2.Column - p1.Column));
        }


        public int Row;
        public int Column;

        public Point(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public void Deconstruct(out int row, out int column)
        {
            row = Row;
            column = Column;
        }

        public int CompareTo(object? obj)
        {
            if (obj is not Point point) 
                return 1;

            if (point.Row == Row)
                return Column - point.Column;

            return Row - point.Row;

        }
        
        public override bool Equals(object? ob)
        {
            return ob switch
            {
                Point other => Row == other.Row && Column == other.Column,
                _ => false
            };
        }

        public override int GetHashCode(){
            return Row.GetHashCode() ^ Column.GetHashCode();
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }
    }
}
