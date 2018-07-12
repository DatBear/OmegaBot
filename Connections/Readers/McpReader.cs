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
            while (_connection.Socket.Connected)
            {
                if (!_connection.GetPacket(ref mcpBuffer, ref  data))
                {
                    break;
                }
                
                lock (_connection.Packets)
                {
                    _connection.Packets.Enqueue(1, data);
                }
                _connection.PacketsReady.Set();
            }
        }
    }
}
