﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class GameData
    {
        /*
         * 
         *  Enumerations and Static Consts
         * 
         * 
         */

        public enum CharacterSkillSetupType
        {
            UNKNOWN_SETUP,
            SORCERESS_LIGHTNING,
            SORCERESS_METEOR,
            SORCERESS_ORB,
            SORCERESS_BLIZZARD,
            SORCERESS_METEORB,
            PALADIN_HAMMERDIN,
            PALADIN_SMITER
        };

        public const Int32 InventoryWidth = 10;
        public const Int32 InventoryHeight = 4;

        public const Int32 StashWidth = 6;
        public const Int32 StashHeight = 8;

        public const Int32 CubeWidth = 3;
        public const Int32 CubeHeight = 4;


        public GameData()
        {
            SkillLevels = new Dictionary<Skills.Type, uint>();
            ItemSkillLevels = new Dictionary<Skills.Type, uint>();
            Players = new Dictionary<uint, Player>();
            Npcs = new Dictionary<uint, NpcEntity>();
            Items = new Dictionary<uint, Item>();
            WorldObjects = new Dictionary<uint, WorldObject>();
        }
        /*
         * 
         * Members and Properties
         * 
         */
        public bool InGame;
        private CharacterSkillSetupType _skillSetup;
        public CharacterSkillSetupType CharacterSkillSetup { get { return _skillSetup; } set { _skillSetup = value; } }

        protected Globals.ActType _currentAct;
        public Globals.ActType CurrentAct { get { return _currentAct; } set { _currentAct = value; } }

        protected Int32 _mapId;
        public Int32 MapId { get { return _mapId; } set { _mapId = value; } }
        protected Int32 _areaId;
        public Int32 AreaId { get { return _areaId; } set { _areaId = value; } }

        protected Boolean _fullyEnteredGame;
        public Boolean FullyEnteredGame { get { return _fullyEnteredGame; } set { _fullyEnteredGame = value; } }

        protected Boolean _talkedToNpc;
        public Boolean TalkedToNpc { get { return _talkedToNpc; } set { _talkedToNpc = value; } }

        protected Int32 _lastTeleport;
        public Int32 LastTeleport { get { return _lastTeleport; } set { _lastTeleport = value; } }

        protected UInt32 _experience;
        public UInt32 Experience { get { return _experience; } set { _experience = value; } }

        protected UInt32 _chickenLife;
        public UInt32 ChickenLife { get { return _chickenLife; } set { _chickenLife = value; } }

        protected UInt32 _potLife;
        public UInt32 PotLife { get { return _potLife; } set { _potLife = value; } }

        protected UInt32 _rightSkill;
        public UInt32 RightSkill { get { return _rightSkill; } set { _rightSkill = value; } }

        protected WorldObject _rogueEncampmentWp;
        public WorldObject RogueEncampmentWp { get { return _rogueEncampmentWp; } set { _rogueEncampmentWp = value; } }

        protected WorldObject _redPortal;
        public WorldObject RedPortal { get { return _redPortal; } set { _redPortal = value; } }

        protected Player _me;
        public Player Me { get { return _me; } set { _me = value; } }

        private Dictionary<Skills.Type, UInt32> _skillLevels;
        public Dictionary<Skills.Type, UInt32> SkillLevels { get { return _skillLevels; } set { _skillLevels = value; } }

        private Dictionary<Skills.Type, UInt32> _itemSkillLevels;
        public Dictionary<Skills.Type, UInt32> ItemSkillLevels { get { return _itemSkillLevels; } set { _itemSkillLevels = value; } }

        private Dictionary<UInt32, Player> _players;
        public Dictionary<UInt32, Player> Players { get { return _players; } set { _players = value; } }

        private Dictionary<UInt32, NpcEntity> _npcs;
        public Dictionary<UInt32, NpcEntity> Npcs { get { return _npcs; } set { _npcs = value; } }

        private Dictionary<UInt32, WorldObject> _objects;
        public Dictionary<UInt32, WorldObject> WorldObjects { get { return _objects; } set { _objects = value; } }

        private Dictionary<UInt32, Item> _items;
        public Dictionary<UInt32, Item> Items { get { return _items; } set { _items = value; } }

        private Container _inventory;
        public Container Inventory { get { return _inventory; } set { _inventory = value; } }

        private Container _stash;
        public Container Stash { get { return _stash; } set { _stash = value; } }

        private Container _cube;
        public Container Cube { get { return _cube; } set { _cube = value; } }

        private Container _belt;
        public Container Belt { get { return _belt; } set { _belt = value; } }

        private Int32 _malahId;
        public Int32 MalahId { get { return _malahId; } set { _malahId = value; } }

        private UInt32 _currentLife;
        public UInt32 CurrentLife { get { return _currentLife; } set { _currentLife = value; } }

        protected Boolean _firstNpcInfoPacket;
        public Boolean FirstNpcInfoPacket { get { return _firstNpcInfoPacket; } set { _firstNpcInfoPacket = value; } }

        private Int32 _attackSinceLastTeleport;
        public Int32 AttacksSinceLastTeleport { get { return _attackSinceLastTeleport; } set { _attackSinceLastTeleport = value; } }

        private Int32 _weaponSet;
        public Int32 WeaponSet { get { return _weaponSet; } set { _weaponSet = value; } }

        protected Boolean _hasMerc;
        public Boolean HasMerc { get { return _hasMerc; } set { _hasMerc = value; } }

        private Int32 _lastTimestamp;
        public Int32 LastTimestamp { get { return _lastTimestamp; } set { _lastTimestamp = value; } }


        public void Init()
        {
            RogueEncampmentWp = null;
            RedPortal = null;
            InGame = false;
            FullyEnteredGame = false;
            LastTeleport = 0;
            Experience = 0;
            Me = new Player();
            Logging.Logger.Write("Reset GameData");

            SkillLevels.Clear();
            ItemSkillLevels.Clear();
            Players.Clear();
            Npcs.Clear();
            Items.Clear();
            WorldObjects.Clear();

            Inventory = new Container("Inventory", GameData.InventoryWidth, GameData.InventoryHeight);
            Stash = new Container("Stash", GameData.StashWidth, GameData.StashHeight);
            Cube = new Container("Cube", GameData.CubeWidth, GameData.CubeHeight);
            Belt = new Container("Belt", 4, 4);//todo depend on belt type...

            MalahId = 0;
            CurrentLife = 0;
            FirstNpcInfoPacket = true;
            AttacksSinceLastTeleport = 0;
            WeaponSet = 0;
            HasMerc = false;
        }

        /*
         * 
         * Methods
         * 
         */
        public Player GetPlayer(UInt32 id)
        {
            if (id == _me.Id)
                return _me;
            else
            {
                Player temp;
                bool success = _players.TryGetValue(id, out temp);

                if (success)
                    return temp;
                else
                {
                    _players.Add(id, new Player());
                    _players[id].Id = id;
                    return _players[id];
                }
            }
        }
    }
}