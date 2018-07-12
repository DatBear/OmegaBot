using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using BattleNet.Logging;

namespace BattleNet.Connections
{
    class D2gsConnection : Connection
    {
        
        protected void StartGameServer(IPAddress ip, List<byte> hash, List<byte> token)
        {

        }

        public override bool Init(IPAddress server, ushort port, List<byte> data)
        {
            _socket.Close();
            _packets.Clear();
            try
            {
                Logger.Write("Connecting to {0}:{1}", server, port);
                _socket = new System.Net.Sockets.TcpClient();
                _socket.Connect(server, port);
                _stream = _socket.GetStream();
                Logger.Write(" Connected");
            }
            catch
            {
                Logger.Write(" Failed To connect");
                return false;
            }
            OnStartThread();
            return true;
        }

        public override byte[] BuildPacket(byte command, params IEnumerable<byte>[] args)
        {
            List<byte> packet = new List<byte>();
            packet.Add((byte)command);

            List<byte> packetArray = new List<byte>();
            foreach (IEnumerable<byte> a in args)
                packetArray.AddRange(a);

            packet.AddRange(packetArray);

            return packet.ToArray();
        }
    }
}
