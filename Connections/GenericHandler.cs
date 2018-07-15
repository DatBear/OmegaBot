using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet.Connections
{
    public class GenericHandler
    {
        public static readonly byte[] Nulls = { 0x00, 0x00, 0x00, 0x00 };
        public static readonly byte[] Ten = { 0x10, 0x00, 0x00, 0x00 };
        public static readonly byte[] Six = { 0x06, 0x00, 0x00, 0x00 };
        public static readonly byte[] Zero = { 0x00 };
        public static readonly byte[] One = { 0x01, 0x00, 0x00, 0x00 };

        protected static readonly String Platform = "68XI";
        protected static readonly string ClassicId = "VD2D";
        protected static readonly string LodId = "PX2D";

        public delegate void PacketDispatcher(byte command, params IEnumerable<byte>[] args);
        public event PacketDispatcher BuildDispatchPacket;

        protected readonly Connection Connection;
        public GenericHandler(Connection conn)
        {
            Connection = conn;
        }

        public delegate void StatusUpdaterHandler(Client.Status status);
        public event StatusUpdaterHandler UpdateStatus = delegate { };
        protected void OnUpdateStatus(Client.Status status)
        {
            UpdateStatus(status);
        }

        public virtual void ThreadFunction()
        {
        }
        
        public void BuildWritePacket(byte command, params IEnumerable<byte>[] args)
        {
            BuildDispatchPacket(command,args);
        }

    }
}
