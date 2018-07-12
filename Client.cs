﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Connections;
using System.Threading;
using BattleNet.Connections.Readers;
using BattleNet.Connections.Handlers;
using System.Net;
using System.Diagnostics;

namespace BattleNet
{
    class Client : IDisposable
    {
#region Members

        Bnet m_bnet;
        D2GS m_d2gs;
        GameDifficulty m_difficulty;

        Thread m_gameCreationThread;
#endregion

#region Constructors
        
        public Client(IPAddress server, String character, String account, String password, 
                      GameDifficulty difficulty, String classicKey, String expansionKey, String exeInfo, 
                      UInt32 chickenLife, UInt32 potLife)
        {
            // Create objects
            m_status = Status.STATUS_UNINITIALIZED;
            m_difficulty = difficulty;
            m_d2gs = new D2GS(character, account, chickenLife, potLife);
            m_bnet = new Bnet(server, character, account, password, 
                              difficulty, classicKey, expansionKey, exeInfo);

            m_gameCreationThread = new Thread(GameCreationThread);

            m_bnet.SubscribeStatusUpdates(UpdateStatus);
            m_bnet.SubscribeGameServerStart(StartGameServer);
            m_bnet.SubscribeClassByteUpdate(m_d2gs.UpdateClassByte);
            m_bnet.SubscribeGameCreationThread(MakeNextGame);
            m_bnet.SubscribeCharacterNameUpdate(m_d2gs.UpdateCharacterName);
            m_d2gs.SubscribeNextGameEvent(MakeNextGame);
            m_gameCreationThread.Name = account + "[CRTN]: ";
            m_gameCreationThread.Start();
        }

#endregion

        protected Status m_status;
        protected NextGameType m_nextGame;

#region Enumerations

        public enum Status
        {
            STATUS_UNINITIALIZED,
            STATUS_INVALID_CD_KEY,
            STATUS_INVALID_EXP_CD_KEY,
            STATUS_KEY_IN_USE,
            STATUS_EXP_KEY_IN_USE,
            STATUS_BANNED_CD_KEY,
            STATUS_BANNED_EXP_CD_KEY,
            STATUS_LOGIN_ERROR,
            STATUS_MCP_LOGON_FAIL,
            STATUS_REALM_DOWN,
            STATUS_ON_MCP,
            STATUS_NOT_IN_GAME
        };
        
        public enum NextGameType
        {
            ITEM_TRANSFER,
            RUN_BOT
        };

        public enum GameDifficulty
        {
            NORMAL,
            NIGHTMARE,
            HELL
        };

        #endregion

        protected void UpdateStatus(Status status)
        {
            Logging.Logger.Write("Status updated: {0}", status);
            m_status = status;
        }

        protected void StartGameServer(IPAddress ip, List<byte> hash, List<byte> token)
        {
            m_d2gs.SetGSInfo(hash, token);
            m_d2gs.Init(ip, Globals.GsPort, null);
        }

        public void Connect()
        {
            m_bnet.Connect();
        }

        protected void MakeNextGame()
        {
            m_makeNextGame.Set();
        }

        protected AutoResetEvent m_makeNextGame;
        public void GameCreationThread()
        {
            try
            {
                Logging.Logger.Write("Starting game creation thread");
                m_makeNextGame = new AutoResetEvent(false);
                m_nextGame = NextGameType.RUN_BOT;
                m_makeNextGame.WaitOne();
                while (true)
                {
                    Logging.Logger.Write("Signalled to start creating a game");
                    Thread.Sleep(Settings.Instance.GameStartDelay() * 1000);
                    switch (m_nextGame)
                    {
                        case NextGameType.RUN_BOT:
                            m_bnet.MakeRandomGame(m_difficulty);
                            break;
                        case NextGameType.ITEM_TRANSFER:
                            //m_bnet.JoinGame();
                            break;
                        default:
                            break;
                    }
                    Logging.Logger.Write("Waiting for next signal");
                    m_makeNextGame.WaitOne();
                }
            }
            catch
            {
                System.Environment.Exit(3);
            }
        }



        #region IDisposable Members

        void IDisposable.Dispose()
        {
            m_bnet.Close();
        }

        #endregion

        static void Main(string[] args)
        {
            //Items.ItemTestHarness.Start();
            Logging.Logger.InitTrace();
            Pickit.InitializePickit();

            
            if (!Settings.Instance.Init(args))
            {
                System.Console.WriteLine("could not parse config file");
                return;
            }

            IPAddress server = System.Net.Dns.GetHostAddresses(Settings.Instance.BattlenetGateway()).First();

            string character = Settings.Instance.BattlenetAccountCharacter();
            string account = Settings.Instance.BattlenetAccountName();
            
            string password = Settings.Instance.BattlenetAccountPassword();

            string d2cdkey = Settings.Instance.CdKeyD2();
            string d2expcdkey = Settings.Instance.CdKeyD2Exp();
            
            uint chickenLife = (uint) Settings.Instance.ChickenLeave();
            uint potLife = (uint) Settings.Instance.ChickenPot();

            GameDifficulty gameDiffuculty = Settings.Instance.Difficulty();
            Client client = new Client(server, character, account, password, gameDiffuculty, d2cdkey, d2expcdkey, "Game.exe 03/09/10 04:10:51 61440", chickenLife, potLife);
            client.Connect();
        }
    }
}
