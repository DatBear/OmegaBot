﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Enums;
using BattleNet.Logging;

namespace BattleNet.Connections.Handlers {
    public class D2gsHandler : GenericDispatcher {
        public D2gsHandler(ref D2gsConnection conn)
            : base(conn) {
        }

        public override void ThreadFunction() {
            _firstInfoPacket = true;
            Logger.Write("D2GS handler started!");
            while (Connection.Socket.Connected) {
                if (Connection.Packets.IsEmpty()) {
                    //_connection.PacketsReady.WaitOne();
                }
                else {
                    List<byte> packet;
                    lock (Connection.Packets) {
                        packet = Connection.Packets.Dequeue();
                    }
                    if (!packet.Any()) {
                        continue;
                    }
                    byte type = packet[0];
                    DispatchPacket(type)(type, packet);
                }
            }
            Logger.Write("D2GS Handler Ending...");
        }

        public delegate void PingStarter();
        public event PingStarter StartPinging = delegate { };

        public override PacketHandler DispatchPacket(byte type) {
            switch (type) {
                case 0x00: return GameLoading;
                case 0x01: return GameFlagsPing;
                case 0x02: return StartPingThread;
                case 0x03: return LoadActData;
                case 0x0c: return NpcUpdate;
                case 0x0f: return PlayerMove;
                case 0x15: return PlayerReassign;
                case 0x1a:
                case 0x1b:
                case 0x1c: return ProcessExperience;
                case 0x1d: return BaseAttribute;
                case 0x21:
                case 0x22: return ItemSkillBonus;
                case 0x26: return ChatMessage;
                case 0x27: return NpcInteraction;
                case 0x59: return InitializePlayer;
                case 0x51: return WorldObject;
                case 0x5b: return PlayerJoins;
                case 0x5c: return PlayerLeaves;
                case 0x67: return NpcMovement;
                case 0x68: return NpcMoveEntity;
                case 0x69: return NpcStateUpdate;
                case 0x6d: return NpcStoppedMoving;
                case 0x7A: return PetOrMercUpdate;
                case 0x81: return PetOrMercAdd;
                case 0x82: return PortalUpdate;
                case 0x8B: return PartyUpdate;
                case 0x8f: return Pong;
                case 0x94: return SkillPacket;
                case 0x95: return LifeManaPacket;
                case 0x97: return WeaponSetSwitched;
                case 0x9c:
                case 0x9d: return ItemAction;
                case 0xac: return NpcAssignment;
                default: return VoidRequest;
            }
        }

        protected void GameLoading(byte type, List<byte> data) {
            Logger.Write("Game is loading, please wait...");
        }

        protected void GameFlagsPing(byte type, List<byte> data) {
            Logger.Write("Game flags ping");
            List<byte> packet = new List<byte>();
            packet.Add(0x6d);
            packet.AddRange(BitConverter.GetBytes((uint)Environment.TickCount));
            packet.AddRange(Nulls);
            packet.AddRange(Nulls);
            /*
             _connection.BuildPacket(0x6d, BitConverter.GetBytes((uint)System.Environment.TickCount),
                              nulls, nulls);
             */
            Connection.Write(packet.ToArray());
        }


        public delegate void ItemUpdate(Item item);
        public event ItemUpdate NewItem;
        public delegate void AddNpcDelegate(NpcEntity npc);
        public event AddNpcDelegate AddNpcEvent;
        public event SkillUpdate UpdateSkillLevel;
        public event NoParams SwapWeaponSet;
        public delegate void LifeUpdate(UInt32 plife);
        public event LifeUpdate UpdateLife;
        public delegate void NoParams();
        public event NoParams UpdateTimestamp;
        public delegate void PortalAssign(UInt32 ownerId, UInt32 portalId);
        public event PortalAssign PortalUpdateEvent;
        public delegate void PartyUpdateDelegate(int owner, PartyAction action);
        public event PartyUpdateDelegate PartyUpdateEvent;
        public delegate void MercenaryUpdate(UInt32 id, UInt32 mercId);
        public event MercenaryUpdate PetOrMercUpdateEvent;
        public delegate void NpcStateUpdateDel(UInt32 id, Coordinate coord, byte life);
        public event NpcStateUpdateDel UpdateNpcState = delegate { };
        public event NpcUpdateDel NpcMoveToTarget;
        public delegate void NpcUpdateDel(UInt32 id, Coordinate coord, bool moving, bool running);
        public event NpcUpdateDel UpdateNpcMovement;
        public event NewPlayer InitMe;
        public delegate void NewPlayer(Player newPlayer);
        public event NewPlayer PlayerEnters;
        public delegate void PlayerLeft(UInt32 id);
        public event PlayerLeft PlayerExited;
        public event NoParams NpcTalkedEvent;
        public delegate void SkillUpdate(Skills.Type skill, byte level);
        public event SkillUpdate UpdateItemSkill;
        public delegate void PlayerLevelSet(byte level);
        public event PlayerLevelSet SetPlayerLevel;
        public delegate void ExperienceUpdate(UInt32 exp);
        public event ExperienceUpdate UpdateExperience;
        public delegate void PlayerPositionUpdate(UInt32 id, Coordinate coords, bool directoryKnown);
        public event PlayerPositionUpdate UpdatePlayerPosition;
        public delegate void NpcLifeUpdate(UInt32 id, byte life);
        public event NpcLifeUpdate UpdateNpcLife;
        public delegate void ActData(Globals.ActType act, Int32 mapId, Int32 areaId);
        public event ActData UpdateActData;
        public delegate void NewEntity(UInt16 type, WorldObject ent);
        public event NewEntity UpdateWorldObject;

        byte _level;
        private bool _firstInfoPacket;
        private bool _talkedToNpc;
        Player _me;

        protected void WorldObject(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            if (packet[1] == 0x02) {
                UInt16 obj = BitConverter.ToUInt16(packet, 6);
                UpdateWorldObject?.Invoke(obj, new WorldObject(BitConverter.ToUInt32(packet, 2),
                                                    obj,
                                                    BitConverter.ToUInt16(packet, 8),
                                                    BitConverter.ToUInt16(packet, 10)));
            }
        }

        protected void StartPingThread(byte type, List<byte> data) {
            Logger.Write("Starting Ping thread");
            Connection.Stream.WriteByte(0x6b);
            StartPinging();
        }

        protected void LoadActData(byte type, List<byte> data) {
            byte[] packet = data.ToArray();

            Logger.Write("Loading Act Data");

            Globals.ActType currentAct = (Globals.ActType)data[1];
            Int32 mapId = BitConverter.ToInt32(packet, 2);
            Int32 areaId = BitConverter.ToInt32(packet, 6);

            UpdateActData?.Invoke(currentAct, mapId, areaId);
            /*
            if (!_fullEntered)
            {
                _fullEntered = true;
                Logger.Write("Fully Entered Game.");
            }
            */
        }


        protected void NpcUpdate(byte type, List<byte> data) {
            UInt32 id = BitConverter.ToUInt32(data.ToArray(), 2);
            UpdateNpcLife?.Invoke(id, data[8]);
            //_owner.BotGameData.Npcs[id].Life = data[8];
        }


        protected void PlayerMove(byte type, List<byte> data) {
            Logger.Write("A player is moving");
            byte[] packet = data.ToArray();
            UInt32 playerId = BitConverter.ToUInt32(packet, 2);
            Coordinate coords = new Coordinate(BitConverter.ToUInt16(packet, 7), BitConverter.ToUInt16(packet, 9));
            UpdatePlayerPosition?.Invoke(playerId, coords, true);
            /*
            Player current_player = _owner.BotGameData.GetPlayer(playerId);
            current_player.Location = new Coordinate(BitConverter.ToUInt16(packet, 7), BitConverter.ToUInt16(packet, 9));
            current_player.DirectoryKnown = true;
             */
        }

        protected void PlayerReassign(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 2);
            Coordinate coords = new Coordinate(BitConverter.ToUInt16(packet, 6), BitConverter.ToUInt16(packet, 8));
            UpdatePlayerPosition?.Invoke(id, coords, true);
            /*
            Player current_player = _owner.BotGameData.GetPlayer(id);
            current_player.Location = new Coordinate(BitConverter.ToUInt16(packet, 6), BitConverter.ToUInt16(packet, 8));
             */
        }

        protected void ProcessExperience(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            UInt32 exp = 0;
            if (type == 0x1a)
                exp = data[1];
            else if (type == 0x1b)
                exp = BitConverter.ToUInt16(packet, 1);
            else if (type == 0x1c)
                exp = BitConverter.ToUInt32(packet, 1);

            UpdateExperience?.Invoke(exp);
        }


        protected void BaseAttribute(byte type, List<byte> data) {
            if (data[1] == 0x0c) {
                SetPlayerLevel?.Invoke(data[2]);
                _level = data[2];
                //Console.WriteLine("Setting Player Level: {0}", data[2]);
            }
        }

        protected void ItemSkillBonus(byte type, List<byte> data) {
            UInt32 skill, amount;
            skill = BitConverter.ToUInt16(data.ToArray(), 7);
            if (type == 0x21)
                amount = data[10];
            else
                amount = data[9];

            //Console.WriteLine("Setting Skill: {0} bonus to {1}", skill, amount);
            UpdateItemSkill?.Invoke((Skills.Type)skill, (byte)amount);
        }

        protected void ChatMessage(byte type, List<byte> data) {
        }


        protected void NpcInteraction(byte type, List<byte> data) {
            if (_firstInfoPacket)
                _firstInfoPacket = false;
            else {
                Logger.Write("{0}: [D2GS] Talking to an NPC.");
                _talkedToNpc = true;
                UInt32 id = BitConverter.ToUInt32(data.ToArray(), 2);
                Connection.Write(Connection.BuildPacket(0x2f, One, BitConverter.GetBytes(id)));
                NpcTalkedEvent?.Invoke();
            }
        }

        protected void PlayerLeaves(byte type, List<byte> data) {
            UInt32 id = BitConverter.ToUInt32(data.ToArray(), 1);
            PlayerExited?.Invoke(id);
            //_owner.BotGameData.Players.Remove(id);
        }

        protected void PartyUpdate(byte type, List<byte> data) {
            byte[] packet = data.ToArray();

            //Logger.Write($"{nameof(PartyUpdate)}: [{type:X}] [{BitConverter.ToString(packet)}]");
            //Cancel: [8B-02-00-00-00-00]
            //Request: [8B-02-00-00-00-02]
            int playerId = BitConverter.ToInt32(packet, 1);
            PartyAction action = (PartyAction) packet.Skip(5).First();
            //Logger.Write($"{nameof(PartyUpdate)}: pid:{playerId}, action: {action}");
            PartyUpdateEvent?.Invoke(playerId, action);
        }

        protected void PlayerJoins(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 3);
            if (id != _me.Id) {
                String name = Encoding.UTF8.GetString(packet.Skip(8).Take(15).Where(x => x != 0).ToArray());// BitConverter.ToString(packet, 8, 15);
                Globals.CharacterClassType charClass = (Globals.CharacterClassType)data[7];
                UInt32 level = BitConverter.ToUInt16(packet, 24);
                Player newPlayer = new Player(name, id, charClass, level);
                PlayerEnters?.Invoke(newPlayer);
                //_owner.BotGameData.Players.Add(id, newPlayer);
            }
        }

        protected void InitializePlayer(byte type, List<byte> data) {
            Logger.Write($"INITIALIZE PLAYER TYPE:{type}");
            if (_me != null) return;//already initialized.
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 1);
            Globals.CharacterClassType charClass = (Globals.CharacterClassType)data[5];
            String name = BitConverter.ToString(packet, 6, 15);
            UInt16 x = BitConverter.ToUInt16(packet, 22);
            UInt16 y = BitConverter.ToUInt16(packet, 24);
            Player newPlayer = new Player(name, id, charClass, _level, x, y);
            Logger.Write($"INITIALIZE PLAYER ID:{id}");
            _me = newPlayer;
            InitMe?.Invoke(_me);
        }

        protected void NpcMovement(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 1);
            byte movementType = packet[5];
            UInt16 x = BitConverter.ToUInt16(packet, 6);
            UInt16 y = BitConverter.ToUInt16(packet, 8);
            bool running;
            if (movementType == 0x17)
                running = true;
            else if (movementType == 0x01)
                running = false;
            else
                return;

            UpdateNpcMovement?.Invoke(id, new Coordinate(x, y), true, running);
        }

        protected void NpcMoveEntity(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 1);
            byte movementType = packet[5];
            UInt16 x = BitConverter.ToUInt16(packet, 6);
            UInt16 y = BitConverter.ToUInt16(packet, 8);
            bool running;
            if (movementType == 0x18)
                running = true;
            else if (movementType == 0x00)
                running = false;
            else
                return;

            NpcMoveToTarget?.Invoke(id, new Coordinate(x, y), true, running);
        }

        protected void NpcStateUpdate(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 1);
            byte state = packet[5];

            byte life;
            if (state == 0x09 || state == 0x08)
                life = 0;
            else
                life = packet[10];

            UpdateNpcState?.Invoke(id, new Coordinate(BitConverter.ToUInt16(packet, 6),
                               BitConverter.ToUInt16(packet, 8)), life);
        }

        protected void NpcStoppedMoving(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            UInt32 id = BitConverter.ToUInt32(packet, 1);
            UInt16 x = BitConverter.ToUInt16(packet, 5);
            UInt16 y = BitConverter.ToUInt16(packet, 7);
            byte life = packet[9];

            UpdateNpcMovement?.Invoke(id, new Coordinate(x, y), false, false);
            UpdateNpcLife?.Invoke(id, life);
        }

        protected void PetOrMercAdd(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            Logger.Write($"{nameof(PetOrMercAdd)}: [{type:X}] [{BitConverter.ToString(packet)}]");
            UInt32 id = BitConverter.ToUInt32(packet, 4);
            UInt32 mercId = BitConverter.ToUInt32(packet, 8);

            PetOrMercUpdateEvent?.Invoke(id, mercId);
        }

        protected void PetOrMercUpdate(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            Logger.Write($"{nameof(PetOrMercUpdate)}: [{type:X}] [{BitConverter.ToString(packet)}]");
        }

        protected void PortalUpdate(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            int offset = 5;
            UInt32 ownerId = BitConverter.ToUInt32(packet, 1);

            PortalUpdateEvent?.Invoke(ownerId, BitConverter.ToUInt32(packet, 21));

            //String name = System.Text.Encoding.ASCII.GetString(packet, offset, 15);
            /*
            if (name.Substring(0, _owner.Me.Name.Length) == _owner.BotGameData.Me.Name)
            {
                Logger.Write("Received new portal id");
                _owner.BotGameData.Me.PortalId = BitConverter.ToUInt32(packet, 21);
            }
             */
        }


        protected void Pong(byte type, List<byte> data) {
            UpdateTimestamp?.Invoke();
        }

        protected void LifeManaPacket(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            if (BitConverter.ToUInt16(packet, 6) == 0x0000)
                return;

            UInt32 plife = (uint)BitConverter.ToUInt16(packet, 1) & 0x7FFF;

            UpdateLife?.Invoke(plife);
        }

        protected void WeaponSetSwitched(byte type, List<byte> data) {
            SwapWeaponSet?.Invoke();
        }


        protected void SkillPacket(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            byte skillCount = packet[1];
            int offset = 6;
            for (int i = 0; i < skillCount; i++) {
                UInt16 skill = BitConverter.ToUInt16(packet, offset);
                byte level = packet[offset + 2];
                UpdateSkillLevel?.Invoke((Skills.Type)skill, level);
                offset += 3;
            }
        }

        protected void ItemAction(byte type, List<byte> data) {
            Item item = Items.Parser.Parse(data);
            NewItem?.Invoke(item);
        }

        public static bool BitScanReverse(out int index, ulong mask) {
            index = 0;
            while (mask > 1) {
                mask >>= 1;
                index++;
            }
            return mask == 1;
        }

        protected void NpcAssignment(byte type, List<byte> data) {
            byte[] packet = data.ToArray();
            NpcEntity output;
            //try
            //{
            BitReader br = new BitReader(data.ToArray());
            br.ReadBitsLittleEndian(8);
            UInt32 id = (uint)br.Read(32);
            UInt16 npctype = (ushort)br.Read(16);
            UInt16 x = (ushort)br.Read(16);
            UInt16 y = (ushort)br.Read(16);
            byte life = (byte)br.Read(8);
            byte size = (byte)br.Read(8);

            output = new NpcEntity(id, npctype, life, x, y);

            int informationLength = 16;

            String[] entries;

            if (!DataManager.Instance._monsterFields.Get(npctype, out entries))
                Logger.Write("Failed to read monstats data for NPC of type {0}", type);
            if (entries.Length != informationLength)
                Logger.Write("Invalid monstats entry for NPC of type {0}", type);

            bool lookupName = false;

            if (data.Count > 0x10) {
                br.Read(4);
                if (br.ReadBit()) {
                    for (int i = 0; i < informationLength; i++) {
                        int temp;

                        int value = Int32.Parse(entries[i]);

                        if (!BitScanReverse(out temp, (uint)value - 1))
                            temp = 0;
                        if (temp == 31)
                            temp = 0;

                        //Console.WriteLine("BSR: {0} Bitcount: {1}", temp+1, bitCount);
                        int bits = br.Read(temp + 1);
                    }
                }

                output.SuperUnique = false;

                output.HasFlags = br.ReadBit();
                if (output.HasFlags) {
                    output.Champion = br.ReadBit();
                    output.Unique = br.ReadBit();
                    output.SuperUnique = br.ReadBit();
                    output.IsMinion = br.ReadBit();
                    output.Ghostly = br.ReadBit();
                    //Console.WriteLine("{0} {1} {2} {3} {4}", output.Champion, output.Unique, output.SuperUnique, output.IsMinion, output.Ghostly);
                }

                if (output.SuperUnique) {
                    output.SuperUniqueId = br.ReadBitsLittleEndian(16);
                    String name;
                    if (!DataManager.Instance._superUniques.Get(output.SuperUniqueId, out name)) {
                        Logger.Write("Failed to lookup super unique monster name for {0}", output.SuperUniqueId);
                        output.Name = "invalid";
                    }
                    else {
                        output.Name = name;
                        //Console.WriteLine("NPC: {0}", name);
                    }
                }
                else
                    lookupName = true;

                if (data.Count > 17 && lookupName != true && output.Name != "invalid") {
                    output.IsLightning = false;
                    while (true) {
                        byte mod = (byte)br.ReadBitsLittleEndian(8);
                        if (mod == 0)
                            break;
                        if (mod == 0x11)
                            output.IsLightning = true;
                    }
                }
            }
            else
                lookupName = true;

            if (lookupName) {
                String name;
                if (!DataManager.Instance._monsterNames.Get((int)output.Type, out name))
                    Console.WriteLine("Failed to Look up monster name for {0}", output.Type);
                else
                    output.Name = name;

                //Console.WriteLine("NPC: {0}", name);
            }

            AddNpcEvent?.Invoke(output);
        }
        protected void VoidRequest(byte type, List<byte> data) {
            Logger.Write("Unknown Packet 0x{0:X2} received!", type);
        }

    }
}