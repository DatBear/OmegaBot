using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    public class Coordinate
    {
        public ushort X { get; set; }
        public ushort Y { get; set; }

        public Coordinate()
        {
        }

        public Coordinate(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return this == (Coordinate)obj;
        }

        public override int GetHashCode()
        {
            return X^Y;
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
            Double x2 = Math.Pow(X - other.X, 2.0);
            Double y2 = Math.Pow(Y - other.Y, 2.0);
            Double distance = Math.Sqrt(x2 + y2);
            return distance;
        }

        public byte[] ToBytes() {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(X));
            bytes.AddRange(BitConverter.GetBytes(Y));
            return bytes.ToArray();
        }

        public override string ToString() {
            return $"[{X},{Y}]";
        }
    }
}
