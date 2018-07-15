using System;
using System.Collections.Generic;
using BattleNet.Enums;

namespace BattleNet
{
    class Actions
    {
        private static byte[] BuildPacket(byte command, params IEnumerable<byte>[] args)
        {
            List<byte> packet = new List<byte>();
            packet.Add(command);

            List<byte> packetArray = new List<byte>();
            foreach (IEnumerable<byte> a in args)
                packetArray.AddRange(a);

            packet.AddRange(packetArray);

            return packet.ToArray();
        }

        private static byte[] Byte(object soloByte) {
            return new[] { (byte) soloByte };
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
            return BuildPacket(0x0c, player.Location.ToBytes());
        }

        public static byte[] CastOnObject(UInt32 id)
        {
            return BuildPacket(0x0d, Connections.GenericHandler.One, 
                                BitConverter.GetBytes(id));
        }

        public static byte[] RequestReassignment(UInt32 id)
        {
            return BuildPacket(0x4b, Connections.GenericHandler.Nulls, 
                BitConverter.GetBytes(id));
        }

        public static byte[] TerminateEntityChat(UInt32 id)
        {
            return BuildPacket(0x30, Connections.GenericHandler.One, 
                                BitConverter.GetBytes(id));
        }

        public static byte[] MakeEntityMove(UInt32 id, Coordinate coords)
        {
            return BuildPacket(0x59, Connections.GenericHandler.One, BitConverter.GetBytes(id),
                                BitConverter.GetBytes(coords.X), Connections.GenericHandler.Zero, Connections.GenericHandler.Zero,
                                BitConverter.GetBytes(coords.Y), Connections.GenericHandler.Zero, Connections.GenericHandler.Zero);
        }

        public static byte[] SwitchSkill(UInt32 skill)
        {
            byte[] temp = { 0xFF, 0xFF, 0xFF, 0xFF };
            return BuildPacket(0x3c, BitConverter.GetBytes(skill), temp);
        }

        public static byte[] DrinkPotion(UInt32 id)
        {
            return BuildPacket(0x26, BitConverter.GetBytes(id), Connections.GenericHandler.Nulls,
                                Connections.GenericHandler.Nulls);
        }

        public static byte[] ItemToCursor(uint itemId) {
            return BuildPacket(0x19, BitConverter.GetBytes(itemId));
        }

        public static byte[] ItemToContainer(uint itemId, Coordinate location, ContainerType container) {
            return BuildPacket(0x18, BitConverter.GetBytes(itemId), location.ToBytes(), BitConverter.GetBytes((int) container));
        }

        public static byte[] PartyAction(int playerId, PartyAction action) {
            return BuildPacket(0x5E, Byte(action), BitConverter.GetBytes(playerId));
        }
        
    }
}
