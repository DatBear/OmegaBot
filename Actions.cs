using System;
using System.Collections.Generic;

namespace BattleNet
{
    class Actions
    {
        private static byte[] BuildPacket(byte command, params IEnumerable<byte>[] args)
        {
            List<byte> packet = new List<byte>();
            packet.Add((byte)command);

            List<byte> packetArray = new List<byte>();
            foreach (IEnumerable<byte> a in args)
                packetArray.AddRange(a);

            packet.AddRange(packetArray);

            return packet.ToArray();
        }

        public static byte[] Walk(Coordinate coords)
        {
            return BuildPacket(0x01, coords.ToBytes());
        }

        public static byte[] Run(Coordinate coords)
        {
            return BuildPacket(0x03, coords.ToBytes());
        }

        public static byte[] Relocate(Coordinate coords)
        {
            return BuildPacket(0x5f, coords.ToBytes());
        }

        public static byte[] CastOnCoord(UInt16 x, UInt16 y)
        {
            byte[] packet = BuildPacket(0x0c, BitConverter.GetBytes(x), BitConverter.GetBytes(y));
            return packet;
        }

        public static byte[] CastOnPlayer(Player player)
        {
            return BuildPacket(0x0c, BitConverter.GetBytes((UInt16)player.Location.X),
                                BitConverter.GetBytes((UInt16)player.Location.Y));
        }

        public static byte[] CastOnObject(UInt32 id)
        {
            return BuildPacket(0x0d, Connections.GenericDispatcher.One, 
                                BitConverter.GetBytes(id));
        }

        public static byte[] RequestReassignment(UInt32 id)
        {
            return BuildPacket(0x4b, Connections.GenericDispatcher.Nulls, 
                BitConverter.GetBytes(id));
        }

        public static byte[] TerminateEntityChat(UInt32 id)
        {
            return BuildPacket(0x30, Connections.GenericDispatcher.One, 
                                BitConverter.GetBytes(id));
        }

        public static byte[] MakeEntityMove(UInt32 id, Coordinate coords)
        {
            return BuildPacket(0x59, Connections.GenericDispatcher.One, BitConverter.GetBytes(id),
                                BitConverter.GetBytes(coords.X), Connections.GenericDispatcher.Zero, Connections.GenericDispatcher.Zero,
                                BitConverter.GetBytes(coords.Y), Connections.GenericDispatcher.Zero, Connections.GenericDispatcher.Zero);
        }

        public static byte[] SwitchSkill(UInt32 skill)
        {
            byte[] temp = { 0xFF, 0xFF, 0xFF, 0xFF };
            return BuildPacket(0x3c, BitConverter.GetBytes(skill), temp);
        }

        private static byte[] DrinkPotion(UInt32 id)
        {
            return BuildPacket(0x26, BitConverter.GetBytes(id), Connections.GenericDispatcher.Nulls,
                                Connections.GenericDispatcher.Nulls);
        }

    }
}
