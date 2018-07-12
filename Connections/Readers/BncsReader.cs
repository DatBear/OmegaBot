using System.Collections.Generic;

namespace BattleNet.Connections.Readers {
    class BncsReader : GenericHandler {
        public BncsReader(ref BncsConnection conn)
            : base(conn) {

        }

        public override void ThreadFunction() {
            List<byte> bncsBuffer = new List<byte>();
            List<byte> data = new List<byte>();
            while (Connection.Socket.Connected) {
                if (!Connection.GetPacket(ref bncsBuffer, ref data)) {
                    break;
                }
                lock (Connection.Packets) {
                    Connection.Packets.Enqueue(1, data);
                    Connection.PacketsReady.Set();
                }
            }
        }
    }
}
