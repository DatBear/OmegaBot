using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Connections.Readers;
using BattleNet.Connections.Handlers;
using System.Net;
using System.Threading;
using BattleNet.Logging;

namespace BattleNet.Connections
{
    class D2GS : IDisposable
    {
        D2gsConnection _d2gsConnection;

        D2gsReader _d2gsReader;
        Thread _d2gsReaderThread;
        
        D2gsHandler _d2gsHandler;
        Thread _d2gsHandlerThread;

        GameServerPing _gsPing;
        Thread _gsPingThread;

        GameThread _gameThread;
        Thread _botThread;

        AsciiMap _asciiMap;
        Thread _mapThread;

        public D2GS(String character, String account, UInt32 chickenLife, UInt32 potLife)
        {
            //ConnectedToGs = false;
            _d2gsConnection = new D2gsConnection();
            _d2gsReader = new D2gsReader(ref _d2gsConnection, character);
            _d2gsHandler = new D2gsHandler(ref _d2gsConnection);
            _gsPing = new GameServerPing(ref _d2gsConnection);
            _gameThread = new GameThread(_d2gsConnection, chickenLife, potLife);
            _asciiMap = new AsciiMap(_gameThread.GameData, _d2gsConnection);

            _d2gsConnection.StartThread += delegate {
                _d2gsHandlerThread = new Thread(_d2gsHandler.ThreadFunction);
                _d2gsHandlerThread.Name = account + " [D2Gs]:";
                _d2gsReaderThread = new Thread(_d2gsReader.ThreadFunction);
                _d2gsReaderThread.Name = account + " [D2GS]:";

                _d2gsHandlerThread.Start();
                _d2gsReaderThread.Start();
            };
            
            _d2gsHandler.StartPinging += delegate
            {
                _gsPingThread = new Thread(_gsPing.Run);
                _gsPingThread.Name = account + " [D2GS]:";
                _gsPingThread.Start();
                _mapThread = new Thread(_asciiMap.ThreadFunction);
                _mapThread.Start();
            };

            
            _botThread = new Thread(_gameThread.BotThread);
            _botThread.Name = account + " [BOT]:";
            _botThread.Start();

            SubscribeGameServerEvents();
        }

        public void SubscribeGameServerEvents()
        {
            _d2gsHandler.UpdateActData += delegate(Globals.ActType act, Int32 mapId, Int32 areaId)
            {
                _gameThread.GameData.CurrentAct = act;
                _gameThread.GameData.MapId = mapId;
                _gameThread.GameData.AreaId = areaId;

                if (!_gameThread.GameData.InGame)
                {
                    _gameThread._startRun.Set();
                    _gameThread.GameData.InGame = true;
                }
            };

            _d2gsHandler.UpdateWorldObject += delegate(UInt16 type, WorldObject ent)
            {
                // Pindles portal
                if (type == 0x003c)
                {
                    _gameThread.GameData.RedPortal = ent;
                    Logger.Write("Received red portal ID and coordinates");
                }
                /*
                // A5 WP
                if (type == 0x01ad)
                {
                    _harrogathWp.Id = BitConverter.ToUInt32(packet, 2);
                    _harrogathWp.Location.X = BitConverter.ToUInt16(packet, 8);
                    _harrogathWp.Location.Y = BitConverter.ToUInt16(packet, 10);

                    if (debugging)
                        Console.WriteLine("{0}: [D2GS] Received A5 WP id and coordinates", Account);
                }
                 */
                // A1 WP
                else if (type == 0x0077)
                {
                    _gameThread.GameData.RogueEncampmentWp = ent;
                    Logger.Write("Received A1 WP id and coordinates");
                }
                else
                {
                    if(_gameThread.GameData.WorldObjects.ContainsKey(ent.Id))
                    {
                        _gameThread.GameData.WorldObjects[ent.Id] = ent;
                    }
                    else
                    {
                        _gameThread.GameData.WorldObjects.Add(ent.Id, ent);
                    }
                }
            };
            _d2gsHandler.UpdateExperience += delegate(UInt32 exp) { _gameThread.GameData.Experience += exp; };
            _d2gsHandler.SetPlayerLevel += delegate(byte level) { _gameThread.GameData.Me.Level = level; };
            _d2gsHandler.UpdateItemSkill += delegate(Skills.Type skill, byte level)
            {
                if(_gameThread.GameData.ItemSkillLevels.ContainsKey(skill))
                {
                    _gameThread.GameData.ItemSkillLevels[skill] += level;
                }
                else
                {
                    _gameThread.GameData.ItemSkillLevels.Add(skill,level);
                }
                
            };
            _d2gsHandler.UpdateNpcLife += delegate(UInt32 id, byte life)
            {
                _gameThread.GameData.Npcs[id].Life = life;
            };
            _d2gsHandler.UpdatePlayerPosition += delegate(UInt32 id, Coordinate coords, bool directoryKnown)
            {
                Player player = _gameThread.GameData.GetPlayer(id);
                player.Location = coords;
                player.DirectoryKnown = directoryKnown;
            };
            _d2gsHandler.PlayerExited += delegate(UInt32 id)
            {
                if (_gameThread.GameData.Players.ContainsKey(id))
                {
                    _gameThread.GameData.Players.Remove(id);
                }
            };
            _d2gsHandler.PlayerEnters += delegate(Player player)
            {
                Logging.Logger.Write("Adding new Player {0}", player.Name);
                if (_gameThread.GameData.Players.ContainsKey(player.Id))
                {
                    _gameThread.GameData.Players[player.Id] = player;
                }
                else
                {
                    _gameThread.GameData.Players.Add(player.Id, player);
                }
            };
            _d2gsHandler.InitMe += delegate(Player player)
            {
                Logging.Logger.Write("Initializing Self");
                _gameThread.GameData.Me = player;
            };
            _d2gsHandler.UpdateNpcMovement += delegate(UInt32 id, Coordinate coord, bool moving, bool running)
            {
                NpcEntity npc = _gameThread.GameData.Npcs[id];
                npc.Location = coord;
                npc.Moving = moving;
                npc.Running = running;
            };
            _d2gsHandler.NpcMoveToTarget += delegate(UInt32 id, Coordinate coord, bool moving, bool running)
            {
                NpcEntity npc = _gameThread.GameData.Npcs[id];
                npc.Location = coord;
                npc.Moving = moving;
                npc.Running = running;
            };
            _d2gsHandler.UpdateNpcState += delegate(UInt32 id, Coordinate coord, byte life)
            {
                //Logging.Logger.Write("Updating NPC {0}, ({1},{2}), Life:{3}", id, coord.X, coord.Y, life);
                NpcEntity npc = _gameThread.GameData.Npcs[id];
                npc.Location = coord;
                npc.Life = life;
            };
            _d2gsHandler.MercUpdateEvent += delegate(UInt32 id, UInt32 mercId)
            {
                Logging.Logger.Write("Mercenary for 0x{0:X} found your id: 0x{1:X}", id, _gameThread.GameData.Me.Id);
                if (id == _gameThread.GameData.Me.Id)
                {
                    _gameThread.GameData.Me.MercenaryId = mercId;
                    _gameThread.GameData.Me.HasMecenary = true;
                    _gameThread.GameData.HasMerc = true;
                }
                else
                {
                    _gameThread.GameData.Players[id].MercenaryId = mercId;
                    _gameThread.GameData.Players[id].HasMecenary = true;
                }
            };

            _d2gsHandler.PortalUpdateEvent += delegate(UInt32 ownerId, UInt32 portalId)
            {
                Logging.Logger.Write("Town Portal belonging to 0x{0:X} found ", ownerId);
                if (ownerId == _gameThread.GameData.Me.Id)
                {
                    _gameThread.GameData.Me.PortalId = portalId;
                }
                else
                {
                    _gameThread.GameData.Players[ownerId].PortalId = portalId;
                }
            };
            _d2gsHandler.UpdateTimestamp += delegate { _gameThread.GameData.LastTimestamp = (int)System.DateTime.Now.ToFileTimeUtc(); };

            _d2gsHandler.SwapWeaponSet += delegate { _gameThread.GameData.WeaponSet = _gameThread.GameData.WeaponSet == 0 ? 1 : 0; };

            _d2gsHandler.UpdateSkillLevel += delegate(Skills.Type skill, byte level)
            {
                Logging.Logger.Write("Adding new Skill {0}:{1}", skill, level);
                _gameThread.GameData.SkillLevels.Add(skill, level);
            };

            _d2gsHandler.UpdateItemSkill += delegate(Skills.Type skill, byte level)
            {
                _gameThread.GameData.ItemSkillLevels[(Skills.Type)skill] = level;
            };

            _d2gsHandler.UpdateLife += delegate(UInt32 plife)
            {
                if (_gameThread.GameData.CurrentLife == 0)
                    _gameThread.GameData.CurrentLife = plife;

                if (plife < _gameThread.GameData.CurrentLife && plife > 0)
                {
                    UInt32 damage = _gameThread.GameData.CurrentLife - plife;
                    Logger.Write("{0} damage was dealt to {1} ({2} left)", damage, _gameThread.GameData.Me.Name, plife);
                    if (plife <= _gameThread.GameData.ChickenLife)
                    {
                        Logger.Write("Chickening with {0} left!", plife);

                        Logger.Write("Leaving the game.");
                        _gameThread.SendPacket((byte)0x69);

                        Thread.Sleep(500);
                        _d2gsConnection.Kill();
                    }
                    else if (plife <= _gameThread.GameData.PotLife)
                    {
                        Logger.Write("Attempting to use potion with {0} life left.", plife);
                        _gameThread.UsePotion();
                    }
                }

                _gameThread.GameData.CurrentLife = plife;
            };

            _d2gsHandler.NewItem += delegate(Item item)
            {
                lock (_gameThread.GameData.Items)
                {
                    if (_gameThread.GameData.Items.ContainsKey(item.id))
                        _gameThread.GameData.Items[item.id] = item;
                    else
                        _gameThread.GameData.Items.Add(item.id, item);
                    
                }
                if (!item.ground && !item.unspecified_directory)
                {
                    switch (item.container)
                    {
                        case Item.ContainerType.inventory:
                            _gameThread.GameData.Inventory.Add(item);
                            //Console.WriteLine("New Item in Inventory!");
                            break;
                        case Item.ContainerType.cube:
                            _gameThread.GameData.Cube.Add(item);
                            break;
                        case Item.ContainerType.stash:
                            _gameThread.GameData.Stash.Add(item);
                            break;
                        case Item.ContainerType.belt:
                            _gameThread.GameData.Belt.Add(item);
                            break;
                    }
                }
            };

            _d2gsHandler.AddNpcEvent += delegate(NpcEntity npc)
            {
                //Logging.Logger.Write("Adding new NPC {0}", npc);
                if (_gameThread.GameData.Npcs.ContainsKey(npc.Id))
                    _gameThread.GameData.Npcs[npc.Id] = npc;
                else
                    _gameThread.GameData.Npcs.Add(npc.Id, npc);
            };

            _d2gsHandler.NpcTalkedEvent += delegate 
            {
                Logger.Write("Talked to NPC");
                _gameThread.GameData.TalkedToNpc = true; 
            };
        }

        public void SubscribeNextGameEvent(D2gsReader.NoParam sub)
        {
            _d2gsReader.NextGame += sub;
        }

        public void UpdateCharacterName(String name)
        {
            _d2gsReader.Character = name;
        }
        public void UpdateClassByte(byte classByte)
        {
            _d2gsReader.ClassByte = classByte;
        }

        public void SetGSInfo(List<byte> hash, List<byte> token)
        {
            _d2gsReader.SetInfo(hash, token);
        }
        public void KillAll()
        {
            _d2gsConnection.Kill();
            
            if (_gsPingThread.IsAlive)
                _gsPingThread.Join();
        }
        public void Init(IPAddress ip, ushort port, List<byte> data)
        {
            _gameThread.GameData.Init();
            Logging.Logger.Write("Initializing D2GS");
            if (!_d2gsConnection.Init(ip, port, data))
            {
                _d2gsReader.Die();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _d2gsConnection.Close();
        }

        #endregion
    }
}
