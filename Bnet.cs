using System;
using System.Collections.Generic;
using BattleNet.Connections;
using System.Threading;
using BattleNet.Connections.Handlers;
using BattleNet.Connections.Readers;
using System.Net;

namespace BattleNet
{
    class Bnet : IDisposable
    {
#region Members

        //Holds the connection and stream for the BNCS
        BncsConnection _bncsConnection;

        // Reads packets from the BNCS stream and places it in a queue for the handler
        BncsReader _bncsReader;
        Thread _bncsReaderThread;

        // Handles all the packets received and dispatches them appropriately
        BncsHandler _bncsHandler;
        Thread _bncsHandlerThread;

        //Holds the connection and stream to the MCP
        McpConnection _mcpConnection;

        // This class pulls all MCP packets from the stream and places it
        // into a queue for the handler to use
        McpReader _mcpReader;
        Thread _mcpReaderThread;

        //This class handles all of the received MCP Packets
        McpHandler _mcpHandler;
        Thread _mcpHandlerThread;

        // Battle.Net server we are connecting to for this client
        IPAddress _server;

#endregion

#region Initializers

        private void InitThreads(String account)
        {
            _bncsReaderThread = new Thread(_bncsReader.ThreadFunction);
            _bncsReaderThread.Name = account + " [BNCS]:";

            _bncsHandlerThread = new Thread(_bncsHandler.ThreadFunction);
            _bncsHandlerThread.Name = account + " [BNCS]:";

            _mcpReaderThread = new Thread(_mcpReader.ThreadFunction);
            _mcpReaderThread.Name = account + " [MCP]:";

            _mcpHandlerThread = new Thread(_mcpHandler.ThreadFunction);
            _mcpHandlerThread.Name = account + " [MCP]:";
        }

        private void InitServers(String character, String account, String password, 
                                 Client.GameDifficulty difficulty, String classicKey, 
                                 String expansionKey, String exeInfo)
        {
            _bncsConnection = new BncsConnection();
            _bncsReader = new BncsReader(ref _bncsConnection);
            _bncsHandler = new BncsHandler(ref _bncsConnection, account, password,
                                            classicKey, expansionKey, exeInfo);
            _mcpConnection = new McpConnection();
            _mcpReader = new McpReader(ref _mcpConnection);
            _mcpHandler = new McpHandler(ref _mcpConnection, character, difficulty);
        }

        private void AssociateEvents()
        {
            _bncsConnection.StartThread += _bncsReaderThread.Start;
            _bncsConnection.StartThread += _bncsHandlerThread.Start;

            _mcpConnection.StartThread += _mcpReaderThread.Start;
            _mcpConnection.StartThread += _mcpHandlerThread.Start;

            _bncsHandler.StartMcpThread += StartMcp;
            _bncsHandler.RealmUpdate += _mcpHandler.UpdateRealm;

            _mcpHandler.BuildDispatchPacket += WriteBncsPacket;
        }

#endregion

#region Constructors

        public Bnet(IPAddress server, String character, String account, String password, Client.GameDifficulty difficulty, String classicKey, String expansionKey, String exeInfo)
        {
            _server = server;
            InitServers(character, account, password, difficulty, classicKey, expansionKey, password);
            InitThreads(account);
            AssociateEvents();
        }

#endregion

#region Events
        
        protected void WriteBncsPacket(byte command, params IEnumerable<byte>[] args)
        {
            byte[] packet = _bncsConnection.BuildPacket(command, args);
            _bncsConnection.Write(packet);
        }

        protected void StartMcp(IPAddress server, ushort port, List<byte> data)
        {
            _mcpConnection.Init(server, port, data);
        }

#endregion

#region Subscribers

        public void SubscribeCharacterNameUpdate(McpHandler.CharacterUpdateDel sub)
        {
            _mcpHandler.UpdateCharacterName += sub;
        }
        public void SubscribeGameCreationThread(BncsHandler.GameCreationThreadHandler sub)
        {
            _bncsHandler.StartGameCreationThread += sub;

        }

        public void SubscribeClassByteUpdate(McpHandler.SetByte sub)
        {
            _mcpHandler.SetClassByte += sub;
        }

        public void SubscribeGameServerStart(McpHandler.D2gsStarter sub)
        {
            _mcpHandler.StartGameServer += sub;
        }

        public void SubscribeStatusUpdates(GenericHandler.StatusUpdaterHandler sub)
        {
            _bncsHandler.UpdateStatus += sub;
            _mcpHandler.UpdateStatus += sub;
        }

#endregion

        public void JoinGame(String gameName, String gamePass)
        {
            _mcpHandler.JoinGame(gameName, gamePass);
        }

        public void MakeGame(Client.GameDifficulty difficulty, String gameName, String gamePass)
        {
            _mcpHandler.MakeGame(difficulty, gameName, gamePass);
        }

        public void MakeRandomGame(Client.GameDifficulty difficulty)
        {
            _mcpHandler.MakeRandomGame(difficulty);
        }


        // Connect to the BNCS and MCP
        public bool Connect()
        {
            return _bncsConnection.Init(_server, 6112);
        }

        #region IDisposable Members

        public void Close()
        {
            _bncsConnection.Close();
            _mcpConnection.Close();
        }

        void IDisposable.Dispose()
        {
            _bncsConnection.Close();
            _mcpConnection.Close();
        }

        #endregion
    }
}
