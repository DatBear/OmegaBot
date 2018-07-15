using System;
using System.Collections.Generic;
using System.Linq;
using BattleNet.Connections;
using System.Threading;
using System.Net;
using BattleNet.Logging;

namespace BattleNet
{
    public class Client : IDisposable
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
            _status = Status.StatusUninitialized;
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
        //yikes
        public enum Status
        {
            StatusUninitialized,
            StatusInvalidCdKey,
            StatusInvalidExpCdKey,
            StatusKeyInUse,
            StatusExpKeyInUse,
            StatusBannedCdKey,
            StatusBannedExpCdKey,
            StatusLoginError,
            StatusMcpLogonFail,
            StatusRealmDown,
            StatusOnMcp,
            StatusNotInGame
        };
        
        public enum NextGameType
        {
            ItemTransfer,
            RunBot,
            FollowLeader
        };

        public enum GameDifficulty
        {
            Normal,
            Nightmare,
            Hell
        };

        #endregion

        protected void UpdateStatus(Status status)
        {
            Logger.Write("Status updated: {0}", status);
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
                Logger.Write("Starting game creation thread");
                _makeNextGame = new AutoResetEvent(false);
                _nextGame = NextGameType.RunBot;
                _makeNextGame.WaitOne();
                while (true)
                {
                    Logger.Write("Signalled to start creating a game");
                    Thread.Sleep(Settings.Instance.GameStartDelay() * 1000);
                    switch (_nextGame)
                    {
                        case NextGameType.RunBot:
                            _bnet.MakeGameFromSettings(_difficulty, Settings.Instance.GameName(), Settings.Instance.GamePassword());
                            //_bnet.MakeRandomGame(_difficulty);
                            break;
                        case NextGameType.ItemTransfer:
                            //_bnet.JoinGame();
                            break;
                        default:
                            break;
                    }
                    Logger.Write("Waiting for next signal");
                    _makeNextGame.WaitOne();
                }
            }
            catch
            {
                Environment.Exit(3);
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
            Logger.InitTrace();
            Pickit.InitializePickit();

            
            if (!Settings.Instance.Init(args))
            {
                Console.WriteLine("could not parse config file");
                return;
            }

            IPAddress server = Dns.GetHostAddresses(Settings.Instance.BattlenetGateway()).First();

            string character = Settings.Instance.BattlenetAccountCharacter();
            string account = Settings.Instance.BattlenetAccountName();
            
            string password = Settings.Instance.BattlenetAccountPassword();

            string d2CdKey = Settings.Instance.CdKeyD2();
            string d2ExpCdKey = Settings.Instance.CdKeyD2Exp();
            
            uint chickenLife = (uint) Settings.Instance.ChickenLeave();
            uint potLife = (uint) Settings.Instance.ChickenPot();

            GameDifficulty gameDiffuculty = Settings.Instance.Difficulty();
            Client client = new Client(server, character, account, password, gameDiffuculty, d2CdKey, d2ExpCdKey, "Game.exe 03/09/10 04:10:51 61440", chickenLife, potLife);
            client.Connect();
        }
    }
}
