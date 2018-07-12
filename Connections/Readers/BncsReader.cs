using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet.Connections.Readers
{
    class BncsReader : GenericHandler
    {
        public BncsReader(ref BncsConnection conn)
            : base(conn)
        {

        }

        public override void ThreadFunction()
        {
            List<byte> bncsBuffer = new List<byte>();
            List<byte> data = new List<byte>();
            while (_connection.Socket.Connected)
            {
                if(!_connection.GetPacket(ref bncsBuffer,ref data))
                {
                    break;
                }
                lock (_connection.Packets)
                {
                    _connection.Packets.Enqueue(1, data);
                    _connection.PacketsReady.Set();
                }
            }
        }
    }
}
