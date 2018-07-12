using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace BattleNet.Logging.IRC
{
    class Bot
    {
        protected String _server;
        protected UInt16 _port;
        protected String _user;
        protected String _nickname;
        protected String _channel;

        protected NetworkStream _stream;
        protected TcpClient _socket;

        protected StreamWriter _writer;
        public StreamWriter Writer { get { return _writer; } }

        public void Init(String server, UInt16 port, String nickname, String channel)
        {
            _server = server;
            _port = port;
            _nickname = nickname;
            _channel = channel;
            _user = "USER D2Bot" + _nickname + " 8 * :Clientless Bot outputter";
        }

        public void Connect()
        {
            _socket = new TcpClient(_server, _port);
            Thread pingThread = new Thread(PingThread);
            _stream = _socket.GetStream();
            _writer = new StreamWriter(_stream);
            _writer.AutoFlush = true;
            pingThread.Start();
            _writer.WriteLine(_user);
            _writer.Flush();
            _writer.WriteLine("NICK " + _nickname); _writer.Flush();
            _writer.WriteLine("JOIN " + _channel);
            _writer.Flush();
            Thread.Sleep(10000);
            Console.WriteLine("Connected to IRC");
        }

        public void Write(String str)
        {
            _writer.WriteLine("PRIVMSG " + _channel + " " +  str);
        }

        private Bot()
        {
        }

        private static Bot _bot = new Bot();

        public static Bot Instance()
        {
           return _bot;
        }

        private void PingThread()
        {
            while (_socket.Connected)
            {
                Writer.WriteLine("PING " + _server);
                _writer.Flush();
                Thread.Sleep(15000);
            }
        }

    }
}
