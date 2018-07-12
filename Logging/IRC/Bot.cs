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
        protected String Server;
        protected UInt16 Port;
        protected String User;
        protected String Nickname;
        protected String Channel;

        protected NetworkStream Stream;
        protected TcpClient Socket;

        public StreamWriter Writer { get; protected set; }

        public void Init(String server, UInt16 port, String nickname, String channel)
        {
            Server = server;
            Port = port;
            Nickname = nickname;
            Channel = channel;
            User = "USER D2Bot" + Nickname + " 8 * :Clientless Bot outputter";
        }

        public void Connect()
        {
            Socket = new TcpClient(Server, Port);
            Thread pingThread = new Thread(PingThread);
            Stream = Socket.GetStream();
            Writer = new StreamWriter(Stream);
            Writer.AutoFlush = true;
            pingThread.Start();
            Writer.WriteLine(User);
            Writer.Flush();
            Writer.WriteLine("NICK " + Nickname); Writer.Flush();
            Writer.WriteLine("JOIN " + Channel);
            Writer.Flush();
            Thread.Sleep(10000);
            Console.WriteLine("Connected to IRC");
        }

        public void Write(String str)
        {
            Writer.WriteLine("PRIVMSG " + Channel + " " +  str);
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
            while (Socket.Connected)
            {
                Writer.WriteLine("PING " + Server);
                Writer.Flush();
                Thread.Sleep(15000);
            }
        }

    }
}
