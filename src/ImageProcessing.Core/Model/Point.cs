using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Core.Model
{
    public struct Point : IComparable
    {
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
    }
}
