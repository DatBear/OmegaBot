﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using BattleNet.Logging;

namespace BattleNet.Connections
{
    class McpConnection : Connection
    {
        public override bool Init(IPAddress server, ushort port, List<byte> data)
        {
            Logger.Write("[MCP] Connecting to {0}:{1}", server, port);
            Socket.Connect(server, port);
            Stream = Socket.GetStream();
            if (!Stream.CanWrite)
            {
                Logger.Write("Failed To connect to {0}:{1}", server, port);
                return false;
            }
            
            Stream.WriteByte(0x01);
            byte[] packet = BuildPacket(0x01, data);
            Write(packet);

            OnStartThread();
            return true;
        }

        public override bool GetPacket(ref List<byte> mcpBuffer, ref List<byte> data)
        {
            while (mcpBuffer.Count < 3)
            {
                try
                {
                    byte temp = (byte)Stream.ReadByte();
                    mcpBuffer.Add(temp);
                }
                catch
                {
                    //Console.WriteLine("\n{0}: [MCP] Lost Connection to MCP", _owner.Account);
                    Kill();
                    return false;
                }
            }

            byte[] bytes = new byte[mcpBuffer.Count];
            mcpBuffer.CopyTo(bytes);

            short packetLength = BitConverter.ToInt16(bytes, 0);

            while (packetLength > mcpBuffer.Count)
            {
                try
                {
                    byte temp = (byte)Stream.ReadByte();
                    mcpBuffer.Add(temp);
                }
                catch
                {
                    //Console.WriteLine("\n{0}: [MCP] Lost Connection to MCP", _owner.Account);
                    Kill();
                    return false;
                }
            }
            data = new List<byte>(mcpBuffer.GetRange(0, packetLength));
            mcpBuffer.RemoveRange(0, packetLength);
            return true;
        }

        public override byte[] BuildPacket(byte command, params IEnumerable<byte>[] args)
        {
            List<byte> packet = new List<byte>();

            List<byte> packetArray = new List<byte>();
            foreach (IEnumerable<byte> a in args)
            {
                packetArray.AddRange(a);
            }

            UInt16 arrayCount = (UInt16)(packetArray.Count + 3);
            packet.AddRange(BitConverter.GetBytes(arrayCount));
            packet.Add(command);
            packet.AddRange(packetArray);

            byte[] bytes = new byte[arrayCount];
            packet.CopyTo(bytes);
            return bytes;
        }
    }
}
