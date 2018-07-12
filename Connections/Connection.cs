using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace BattleNet.Connections
{
    class Connection : IDisposable
    {
        #region Constructors 
        public Connection()
        {
            _socket = new TcpClient();

            _packets = new PriorityQueue<uint, List<byte>>();
            _packetsReady = new AutoResetEvent(false);
        }
        #endregion

        protected AutoResetEvent _packetsReady;
        public AutoResetEvent PacketsReady { get { return _packetsReady; } set { _packetsReady = value; } }
        #region Events

        public delegate void ThreadStarter();
        public event ThreadStarter StartThread = delegate { };

        public void OnStartThread()
        {
            StartThread();
        }

        #endregion

        #region Members
        protected NetworkStream _stream;
        public NetworkStream Stream { get { return _stream; } set { _stream = value; } }

        protected TcpClient _socket;
        public TcpClient Socket { get { return _socket; } set { _socket = value; } }

        protected PriorityQueue<UInt32,List<byte>> _packets;
        public PriorityQueue<UInt32, List<byte>> Packets { get { return _packets; } set { _packets = value; } }

        #endregion

        #region Methods
        public virtual bool Init(IPAddress server, ushort port, List<byte> data = null)
        {
            return false;
        }

        public virtual void Kill()
        {
            _stream.Close();
            _socket.Close();
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
            if (_socket.Connected)
            {
                try
                {
                    _stream.Write(packet, 0, packet.Length);
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
            _stream.Close();
            _socket.Close();
            _packetsReady.Close();
        }

        #endregion
    }
}
