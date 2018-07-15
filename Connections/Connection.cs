using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace BattleNet.Connections
{
    public class Connection : IDisposable
    {
        #region Constructors 
        public Connection()
        {
            Socket = new TcpClient();

            Packets = new PriorityQueue<uint, List<byte>>();
            PacketsReady = new AutoResetEvent(false);
        }
        #endregion

        public AutoResetEvent PacketsReady { get; set; }

        #region Events

        public delegate void ThreadStarter();
        public event ThreadStarter StartThread = delegate { };

        public void OnStartThread()
        {
            StartThread();
        }

        #endregion

        #region Members

        public NetworkStream Stream { get; set; }
        public TcpClient Socket { get; set; }
        public PriorityQueue<UInt32, List<byte>> Packets { get; set; }

        #endregion

        #region Methods
        public virtual bool Init(IPAddress server, ushort port, List<byte> data = null)
        {
            return false;
        }

        public virtual void Kill()
        {
            Stream.Close();
            Socket.Close();
        }

        public void Close()
        {
            Kill();
        }

        public virtual bool GetPacket(ref List<byte> buffer, ref List<byte> data)
        {
            return false;
        }

        public virtual void Write(byte[] packet)
        {
            if (Socket.Connected)
            {
                try
                {
                    Stream.Write(packet, 0, packet.Length);
                }
                catch
                {
                    Kill();
                }
            }
        }

        public virtual byte[] BuildPacket(byte command, params IEnumerable<byte>[] args)
        {
            return null;
        }
        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            Stream.Close();
            Socket.Close();
            PacketsReady.Close();
        }

        #endregion
    }
}
