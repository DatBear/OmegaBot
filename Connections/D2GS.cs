using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Connections.Readers;
using BattleNet.Connections.Handlers;
using System.Net;
using System.Threading;
using BattleNet.Enums;
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
            _gameThread = new GameThread(_d2gsConnection, _d2gsHandler, chickenLife, potLife);
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
            _d2gsHandler.UpdateActData += OnUpdateActData;
            _d2gsHandler.UpdateWorldObject += OnUpdateWorldObject;
            _d2gsHandler.UpdateExperience += OnUpdateExperience;
            _d2gsHandler.SetPlayerLevel += OnSetPlayerLevel;
            _d2gsHandler.UpdateNpcLife += OnUpdateNpcLife;
            _d2gsHandler.UpdatePlayerPosition += OnUpdatePlayerPosition;
            _d2gsHandler.PlayerExited += OnPlayerExited;
            _d2gsHandler.PlayerEnters += OnPlayerEnters;
            _d2gsHandler.InitMe += OnInitMe;
            _d2gsHandler.UpdateNpcMovement += OnUpdateNpcMovement;
            _d2gsHandler.NpcMoveToTarget += OnNpcMoveToTarget;
            _d2gsHandler.UpdateNpcState += OnUpdateNpcState;
            _d2gsHandler.PetOrMercUpdateEvent += OnPetOrMercUpdateEvent;
            _d2gsHandler.PortalUpdateEvent += OnPortalUpdateEvent;
            _d2gsHandler.UpdateTimestamp += OnUpdateTimestamp;
            _d2gsHandler.SwapWeaponSet += OnSwapWeaponSet;
            _d2gsHandler.UpdateSkillLevel += OnUpdateSkillLevel;
            _d2gsHandler.UpdateItemSkill += OnUpdateItemSkill;
            _d2gsHandler.UpdateLife += OnUpdateLife;
            _d2gsHandler.NewItem += OnNewItem;
            _d2gsHandler.AddNpcEvent += OnAddNpcEvent;
            _d2gsHandler.NpcTalkedEvent += OnNpcTalkedEvent;
            _d2gsHandler.PartyUpdateEvent += OnPartyUpdateEvent;
        }

        private void OnPartyUpdateEvent(int owner, PartyAction action) {
            if (action == PartyAction.ServerInvite) {
                
            }
        }

        private void OnUpdateActData(Globals.ActType act, int mapId, int areaId) {
            _gameThread.GameData.CurrentAct = act;
            _gameThread.GameData.MapId = mapId;
            _gameThread.GameData.AreaId = areaId;

            if (!_gameThread.GameData.InGame) {
                _gameThread._startRun.Set();
                _gameThread.GameData.InGame = true;
            }
        }

        private void OnUpdateWorldObject(ushort type, WorldObject ent) {
            // Pindles portal
            if (type == 0x003c) {
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
            else if (type == 0x0077) {
                _gameThread.GameData.RogueEncampmentWp = ent;
                Logger.Write("Received A1 WP id and coordinates");
            }
            else {
                if (_gameThread.GameData.WorldObjects.ContainsKey(ent.Id)) {
                    _gameThread.GameData.WorldObjects[ent.Id] = ent;
                }
                else {
                    _gameThread.GameData.WorldObjects.Add(ent.Id, ent);
                }
            }
        }

        private void OnUpdateExperience(uint exp) {
            _gameThread.GameData.Experience += exp;
        }

        private void OnSetPlayerLevel(byte level) {
            _gameThread.GameData.Me.Level = level;
        }

        private void OnUpdateNpcLife(uint id, byte life) {
            _gameThread.GameData.Npcs[id].Life = life;
        }

        private void OnUpdatePlayerPosition(uint id, Coordinate coords, bool directoryKnown) {
            Player player = _gameThread.GameData.GetPlayer(id);
            player.Location = coords;
            player.DirectoryKnown = directoryKnown;
        }

        private void OnPlayerExited(uint id) {
            if (_gameThread.GameData.Players.ContainsKey(id)) {
                _gameThread.GameData.Players.Remove(id);
            }
        }

        private void OnPlayerEnters(Player player) {
            Logger.Write("Adding new Player {0}", player.Name);
            if (_gameThread.GameData.Players.ContainsKey(player.Id)) {
                _gameThread.GameData.Players[player.Id] = player;
            }
            else {
                _gameThread.GameData.Players.Add(player.Id, player);
            }
        }

        private void OnInitMe(Player player) {
            Logger.Write("Initializing Self");
            _gameThread.GameData.Me = player;
        }

        private void OnUpdateNpcMovement(uint id, Coordinate coord, bool moving, bool running) {
            _gameThread.GameData.Npcs.TryGetValue(id, out NpcEntity npc);
            if (npc == null) return;
            npc.Location = coord;
            npc.Moving = moving;
            npc.Running = running;
        }

        private void OnNpcMoveToTarget(uint id, Coordinate coord, bool moving, bool running) {
            _gameThread.GameData.Npcs.TryGetValue(id, out NpcEntity npc);
            if (npc == null) return;
            npc.Location = coord;
            npc.Moving = moving;
            npc.Running = running;
        }

        private void OnUpdateNpcState(uint id, Coordinate coord, byte life) {
            Logger.Write("Updating NPC {0}, ({1},{2}), Life:{3}", id, coord.X, coord.Y, life);
            NpcEntity npc = _gameThread.GameData.Npcs[id];
            npc.Location = coord;
            npc.Life = life;
        }

        private void OnPetOrMercUpdateEvent(uint id, uint mercId) {
            Logger.Write("Mercenary for 0x{0:X} found your id: 0x{1:X}", id, _gameThread.GameData.Me.Id);
            if (id == _gameThread.GameData.Me.Id) {
                _gameThread.GameData.Me.MercenaryId = mercId;
                _gameThread.GameData.Me.HasMecenary = true;
                _gameThread.GameData.HasMerc = true;
            }
            else {
                _gameThread.GameData.Players.TryGetValue(id, out Player player);
                Logger.Write("Mercenary for {0:X} found merc id: {1:X}", id, mercId);
                if (player == null) return;
                player.MercenaryId = mercId;
                player.HasMecenary = true;
            }
        }

        private void OnPortalUpdateEvent(uint ownerId, uint portalId) {
            Logger.Write("Town Portal belonging to 0x{0:X} found ", ownerId);
            if (ownerId == _gameThread.GameData.Me.Id) {
                _gameThread.GameData.Me.PortalId = portalId;
            }
            else {
                _gameThread.GameData.Players[ownerId].PortalId = portalId;
            }
        }

        private void OnUpdateTimestamp() {
            _gameThread.GameData.LastTimestamp = (int) System.DateTime.Now.ToFileTimeUtc();
        }

        private void OnSwapWeaponSet() {
            _gameThread.GameData.WeaponSet = _gameThread.GameData.WeaponSet == 0 ? 1 : 0;
        }

        private void OnUpdateSkillLevel(Skills.Type skill, byte level) {
            Logger.Write("Adding new Skill {0}:{1}", skill, level);
            _gameThread.GameData.SkillLevels.Add(skill, level);
        }

        private void OnUpdateItemSkill(Skills.Type skill, byte level) {
            //_gameThread.GameData.ItemSkillLevels[(Skills.Type) skill] = level;
            if (_gameThread.GameData.ItemSkillLevels.ContainsKey(skill)) {
                _gameThread.GameData.ItemSkillLevels[skill] += level;
            }
            else {
                _gameThread.GameData.ItemSkillLevels.Add(skill, level);
            }
        }

        private void OnUpdateLife(uint plife) {
            if (_gameThread.GameData.CurrentLife == 0)
                _gameThread.GameData.CurrentLife = plife;

            if (plife < _gameThread.GameData.CurrentLife && plife > 0) {
                UInt32 damage = _gameThread.GameData.CurrentLife - plife;
                Logger.Write("{0} damage was dealt to {1} ({2} left)", damage, _gameThread.GameData.Me.Name, plife);
                if (plife <= _gameThread.GameData.ChickenLife) {
                    Logger.Write("Chickening with {0} left!", plife);

                    Logger.Write("Leaving the game.");
                    _gameThread.SendPacket((byte) 0x69);

                    Thread.Sleep(500);
                    _d2gsConnection.Kill();
                }
                else if (plife <= _gameThread.GameData.PotLife) {
                    Logger.Write("Attempting to use potion with {0} life left.", plife);
                    _gameThread.UsePotion();
                }
            }

            _gameThread.GameData.CurrentLife = plife;
        }

        private void OnNewItem(Item item) {
            lock (_gameThread.GameData.Items) {
                if (_gameThread.GameData.Items.ContainsKey(item.Id))
                    _gameThread.GameData.Items[item.Id] = item;
                else
                    _gameThread.GameData.Items.Add(item.Id, item);
            }
            if (!item.Ground && !item.UnspecifiedDirectory) {
                switch (item.Container) {
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
        }

        private void OnAddNpcEvent(NpcEntity npc) {
            //Logger.Write("Adding new NPC {0}", npc);
            if (_gameThread.GameData.Npcs.ContainsKey(npc.Id))
                _gameThread.GameData.Npcs[npc.Id] = npc;
            else
                _gameThread.GameData.Npcs.Add(npc.Id, npc);
        }

        private void OnNpcTalkedEvent() {
            Logger.Write("Talked to NPC");
            _gameThread.GameData.TalkedToNpc = true;
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
            Logger.Write("Initializing D2GS");
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
