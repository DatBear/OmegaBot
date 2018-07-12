using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNet.Logging;

namespace BattleNet {
    class GameData {
        /*
         * 
         *  Enumerations and Static Consts
         * 
         * 
         */
        public enum CharacterSkillSetupType {
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


        public GameData() {
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
        public CharacterSkillSetupType CharacterSkillSetup { get; set; }
        public Globals.ActType CurrentAct { get; set; }
        public Int32 MapId { get; set; }
        public Int32 AreaId { get; set; }
        public Boolean FullyEnteredGame { get; set; }
        public Boolean TalkedToNpc { get; set; }
        public Int32 LastTeleport { get; set; }
        public UInt32 Experience { get; set; }
        public UInt32 ChickenLife { get; set; }
        public UInt32 PotLife { get; set; }
        public UInt32 RightSkill { get; set; }
        public WorldObject RogueEncampmentWp { get; set; }
        public WorldObject RedPortal { get; set; }
        public Player Me { get; set; }
        public Dictionary<Skills.Type, UInt32> SkillLevels { get; set; }
        public Dictionary<Skills.Type, UInt32> ItemSkillLevels { get; set; }
        public Dictionary<UInt32, Player> Players { get; set; }
        public Dictionary<UInt32, NpcEntity> Npcs { get; set; }
        public Dictionary<UInt32, WorldObject> WorldObjects { get; set; }
        public Dictionary<UInt32, Item> Items { get; set; }
        public Container Inventory { get; set; }
        public Container Stash { get; set; }
        public Container Cube { get; set; }
        public Container Belt { get; set; }
        public Int32 MalahId { get; set; }
        public UInt32 CurrentLife { get; set; }
        public bool FirstNpcInfoPacket { get; set; }
        public Int32 AttacksSinceLastTeleport { get; set; }
        public Int32 WeaponSet { get; set; }
        public Boolean HasMerc { get; set; }
        public Int32 LastTimestamp { get; set; }


        public void Init() {
            RogueEncampmentWp = null;
            RedPortal = null;
            InGame = false;
            FullyEnteredGame = false;
            LastTeleport = 0;
            Experience = 0;
            Me = new Player();
            Logger.Write("Reset GameData");

            SkillLevels.Clear();
            ItemSkillLevels.Clear();
            Players.Clear();
            Npcs.Clear();
            Items.Clear();
            WorldObjects.Clear();

            Inventory = new Container("Inventory", InventoryWidth, InventoryHeight);
            Stash = new Container("Stash", StashWidth, StashHeight);
            Cube = new Container("Cube", CubeWidth, CubeHeight);//todo make configurable
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
        public Player GetPlayer(UInt32 id) {
            if (id == Me.Id)
                return Me;
            else {
                Player temp;
                bool success = Players.TryGetValue(id, out temp);

                if (success)
                    return temp;
                else {
                    Players.Add(id, new Player());
                    Players[id].Id = id;
                    return Players[id];
                }
            }
        }
    }
}