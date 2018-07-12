﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using BattleNet.Logging;

namespace BattleNet.Connections.Handlers
{
    class McpHandler : GenericDispatcher
    {
        bool _loggedIn;
        String _character;
        byte _classByte;
        byte _level;
        String _realm;

        Client.GameDifficulty _difficulty;

        String _gameName;
        String _gamePass;
        public McpHandler(ref McpConnection conn, String character, Client.GameDifficulty difficulty)
            : base(conn)
        {
            _loggedIn = false;
            _character = character;
            _classByte = 0;
            _difficulty = difficulty;
        }

        public delegate void SetByte(byte vbyte);
        public event SetByte SetClassByte = delegate { };

        void OnSetClassByte(byte classByte)
        {
            _classByte = classByte;
        }

        public delegate void D2gsStarter(IPAddress ip, List<byte> hash, List<byte> token);
        public event D2gsStarter StartGameServer = delegate { };

        public void UpdateRealm(String realm)
        {
            _realm = realm;
        }

        public override void ThreadFunction()
        {
            Logger.Write("MCP handler started!");
            while (_connection.Socket.Connected)
            {
                _connection.PacketsReady.WaitOne();
                
                List<byte> packet;
                lock (_connection.Packets)
                {
                    packet = _connection.Packets.Dequeue();
                }
                byte type = packet[2];
                DispatchPacket(type)(type, packet);
            }
        }

        public override PacketHandler DispatchPacket(byte type)
        {
            switch (type)
            {
                case 0x01:
                    return LoginRealm;
                case 0x19:
                    return CharacterList;
                case 0x07:
                    return LoginResult;
                case 0x03:
                    return GameJoin;
                case 0x04:
                    return GameServerInfo;
                case 0x12:
                default: return VoidRequest;
            }
        }

        protected void LoginRealm(byte type, List<byte> data)
        {
            UInt32 result = BitConverter.ToUInt32(data.ToArray(), 3);
            switch (result)
            {
                case 0x00:
                    Logger.Write("Successfully Logged on to the Realm Server");
                    break;
                case 0x7e:
                    Logger.Write("Your CD-Key has been banned from this realm!");
                    OnUpdateStatus(Client.Status.STATUS_BANNED_CD_KEY);
                    break;
                case 0x7f:
                    Logger.Write("Your IP has been temporarily banned");
                    OnUpdateStatus(Client.Status.STATUS_REAL_DOWN);
                    break;
                default:
                    break;
            }
            if (result != 0)
                return;

            if (!_loggedIn)
            {
                Logger.Write("Requesting Character list...");
                byte[] packet = _connection.BuildPacket(0x19, BitConverter.GetBytes(8));
                _connection.Write(packet);
            }
            else
            {
                byte[] packet = _connection.BuildPacket(0x07, System.Text.Encoding.ASCII.GetBytes(_character), zero);
                _connection.Write(packet);
            }
        }
        public delegate void CharacterUpdateDel(String chara);
        public event CharacterUpdateDel UpdateCharacterName;
        protected void CharacterList(byte type, List<byte> data)
        {
            UInt16 count = BitConverter.ToUInt16(data.ToArray(), 9);
            if (count == 0)
            {
                OnUpdateStatus(Client.Status.STATUS_ON_MCP);
            }
            else
            {
                bool foundCharacter = false;
                bool selectFirstCharacter = false;
                
                //Console.WriteLine("{0}: [MCP] List of characters on this account", _owner.Account);
                int offset = 11;

                for (int i = 1; i <= count; i++)
                {
                    offset += 4;
                    String dataString = System.Text.Encoding.ASCII.GetString(data.ToArray());
                    String characterName = Utils.readNullTerminatedString(dataString, ref offset);
                    int length = data.IndexOf(0, offset) - offset;
                    List<byte> stats = data.GetRange(offset, length);
                    offset += length;
                    OnSetClassByte((byte)((stats[13] - 0x01) & 0xFF));
                    byte level = stats[25];
                    byte flags = stats[26];
                    bool hardcore = (flags & 0x04) != 0;
                    bool dead = (flags & 0x08) != 0;
                    bool expansion = (flags & 0x20) != 0;
                    String coreString = hardcore ? "Hardcore" : "Softcore";
                    String versionString = expansion ? "Expansion" : "Classic";
                    
                    if (_character == null && i == 1)
                    {
                        selectFirstCharacter = true;
                        _character = characterName;
                        UpdateCharacterName(_character);
                    }

                    if (_character.Equals( characterName))
                    {
                        foundCharacter = true;
                        _level = level;
                    }
                }

                if (!foundCharacter)
                {
                    Logger.Write("Unable to locate character specified");
                    return;
                }

                byte[] packet = _connection.BuildPacket(0x07, System.Text.Encoding.ASCII.GetBytes(_character), zero);
                _connection.Write(packet);
            }
        }

        protected void LoginResult(byte type, List<byte> data)
        {
            UInt32 result = BitConverter.ToUInt32(data.ToArray(), 3);
            if (result != 0)
            {
                Logger.Write("Failed to log into character {0}", _character);
                throw new Exception();
            }

            BuildWritePacket(0x0b, System.Text.Encoding.ASCII.GetBytes(lod_id));
            byte[] comma = { 0x2C };
            BuildWritePacket(0x0a, System.Text.Encoding.ASCII.GetBytes(_character), 
                                   zero, System.Text.Encoding.ASCII.GetBytes(_realm), 
                                   comma, System.Text.Encoding.ASCII.GetBytes(_character), zero);

            if (!_loggedIn)
            {
                byte[] packetc = _connection.BuildPacket(0x12);

                _connection.Write(packetc);
                _loggedIn = true;
            }
            OnUpdateStatus(Client.Status.STATUS_NOT_IN_GAME);
        }

        protected void GameServerInfo(byte type, List<byte> data)
        {
            UInt32 result = BitConverter.ToUInt32(data.ToArray(), 17);

            switch (result)
            {
                case 0x00:
                    Logger.Write("Successfully joined the game");
                    break;
                case 0x12: break; //message of the day
                case 0x29: break;
                case 0x2A: break;
                case 0x2B:
                case 0x2C:
                case 0x71:
                case 0x73:
                case 0x74:
                case 0x78:
                case 0x79:
                case 0x7D:
                    break;
            }

            if (result == 0)
            {
                UInt32 ip = BitConverter.ToUInt32(data.ToArray(), 9);
                StartGameServer(new IPAddress(ip), data.GetRange(13, 4), data.GetRange(5, 2));
            }
        }

        protected void GameJoin(byte type, List<byte> data)
        {
            UInt32 result = BitConverter.ToUInt32(data.ToArray(), 9);
            switch (result)
            {
                case 0x00:
                    Logger.Write("Game has been created successfully");
                    break;
                case 0x1e:
                case 0x1f:
                case 0x20:
                case 0x6e:
                    break;
            }
            if (result == 0)
            {
                _connection.Write(_connection.BuildPacket(0x04, BitConverter.GetBytes((ushort)2), 
                                   System.Text.Encoding.ASCII.GetBytes(_gameName), GenericHandler.zero,
                                   System.Text.Encoding.ASCII.GetBytes(_gamePass), GenericHandler.zero));
            }
        }

        public void JoinGame(String gameName, String gamePass)
        {
            _connection.Write(_connection.BuildPacket(0x04, BitConverter.GetBytes((ushort)2),
                               System.Text.Encoding.ASCII.GetBytes(gameName), GenericHandler.zero,
                               System.Text.Encoding.ASCII.GetBytes(gamePass), GenericHandler.zero));
        }

        public void MakeGame(Client.GameDifficulty difficulty, String gameName, String gamePass)
        {
            _gameName = gameName;
            _gamePass = gamePass;
            byte[] temp = { 0x01, 0xff, 0x08 };
            byte[] packet = _connection.BuildPacket(0x03, BitConverter.GetBytes((UInt16)2), BitConverter.GetBytes(Utils.GetDifficulty(difficulty)), temp, System.Text.Encoding.ASCII.GetBytes(_gameName), GenericHandler.zero,
                            System.Text.Encoding.ASCII.GetBytes(_gamePass), GenericHandler.zero, GenericHandler.zero);
            _connection.Write(packet);
        }

        public void MakeRandomGame(Client.GameDifficulty difficulty)
        {
            String gamePass = Utils.RandomString(3);
            String gameName = Utils.RandomString(10);
            Logger.Write("Create Game {0} with Password {1}", gameName, gamePass);

            MakeGame(difficulty, gameName, gamePass);
        }

        protected void VoidRequest(byte type, List<byte> data)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            //0x12 is MOTD
            Logger.Write("Unknown Packet 0x{0:X2} received!", type);
        }
    }
}
