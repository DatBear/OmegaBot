using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Logging;

namespace BattleNet.Connections.Readers
{
    class McpReader : GenericHandler
    {
        public McpReader(ref McpConnection conn)
            : base(conn)
        {
        }

        public override void ThreadFunction()
        {
            Logger.Write("MCP Reader started!");
            List<byte> data = new List<byte>();
            List<byte> mcpBuffer = new List<byte>();
            while (Connection.Socket.Connected)
            {
                if (!Connection.GetPacket(ref mcpBuffer, ref  data))
                {
                    break;
                }
                
                lock (Connection.Packets)
                {
                    Connection.Packets.Enqueue(1, data);
                }
                Connection.PacketsReady.Set();
            }
        }
    }
}
