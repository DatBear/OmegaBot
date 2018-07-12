using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class Coordinate
    {
        protected UInt16 _x;
        public UInt16 X { get { return _x; } set { _x = value; } }

        protected UInt16 _y;
        public UInt16 Y { get { return _y; } set { _y = value; }  }

        public Coordinate()
        {
        }

        public Coordinate(UInt16 x, UInt16 y)
        {
            _x = x;
            _y = y;
        }

        public override bool Equals(object obj)
        {
            return this == (Coordinate)obj;
        }

        public override int GetHashCode()
        {
            return _x^_y;
        }

        public static bool operator==(Coordinate first, Coordinate second)
        {
            return (first.X == second.X) && (first.Y == second.Y);
        }

        public static bool operator!=(Coordinate first, Coordinate second)
        {
            return !(first == second);
        }

        public Double Distance(Coordinate other)
        {
            Double x2 = Math.Pow(_x - other.X, 2.0);
            Double y2 = Math.Pow(_y - other.Y, 2.0);
            Double distance = Math.Sqrt(x2 + y2);
            return distance;
        }

        public byte[] ToBytes() {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(X));
            bytes.AddRange(BitConverter.GetBytes(Y));
            return bytes.ToArray();
        }

    }
}
