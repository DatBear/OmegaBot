using System;
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

        Bnet _bnet;
        D2GS _d2gs;
        GameDifficulty _difficulty;

        Thread _gameCreationThread;
#endregion

#region Constructors
        
        public Client(IPAddress server, String character, String account, String password, 
                      GameDifficulty difficulty, String classicKey, String expansionKey, String exeInfo, 
                      UInt32 chickenLife, UInt32 potLife)
        {
            // Create objects
            _status = Status.STATUS_UNINITIALIZED;
            _difficulty = difficulty;
            _d2gs = new D2GS(character, account, chickenLife, potLife);
            _bnet = new Bnet(server, character, account, password, 
                              difficulty, classicKey, expansionKey, exeInfo);

            _gameCreationThread = new Thread(GameCreationThread);

            _bnet.SubscribeStatusUpdates(UpdateStatus);
            _bnet.SubscribeGameServerStart(StartGameServer);
            _bnet.SubscribeClassByteUpdate(_d2gs.UpdateClassByte);
            _bnet.SubscribeGameCreationThread(MakeNextGame);
            _bnet.SubscribeCharacterNameUpdate(_d2gs.UpdateCharacterName);
            _d2gs.SubscribeNextGameEvent(MakeNextGame);
            _gameCreationThread.Name = account + "[CRTN]: ";
            _gameCreationThread.Start();
        }

#endregion

        protected Status _status;
        protected NextGameType _nextGame;

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
            STATUS_REAL_DOWN,
            STATUS_ON_MCP,
            STATUS_NOT_IN_GAME
        };
        
        public enum NextGameType
        {
            ITE_TRANSFER,
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
            _status = status;
        }

        protected void StartGameServer(IPAddress ip, List<byte> hash, List<byte> token)
        {
            _d2gs.SetGSInfo(hash, token);
            _d2gs.Init(ip, Globals.GsPort, null);
        }

        public void Connect()
        {
            _bnet.Connect();
        }

        protected void MakeNextGame()
        {
            _makeNextGame.Set();
        }

        protected AutoResetEvent _makeNextGame;
        public void GameCreationThread()
        {
            try
            {
                Logging.Logger.Write("Starting game creation thread");
                _makeNextGame = new AutoResetEvent(false);
                _nextGame = NextGameType.RUN_BOT;
                _makeNextGame.WaitOne();
                while (true)
                {
                    Logging.Logger.Write("Signalled to start creating a game");
                    Thread.Sleep(Settings.Instance.GameStartDelay() * 1000);
                    switch (_nextGame)
                    {
                        case NextGameType.RUN_BOT:
                            _bnet.MakeRandomGame(_difficulty);
                            break;
                        case NextGameType.ITE_TRANSFER:
                            //_bnet.JoinGame();
                            break;
                        default:
                            break;
                    }
                    Logging.Logger.Write("Waiting for next signal");
                    _makeNextGame.WaitOne();
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
            _bnet.Close();
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
