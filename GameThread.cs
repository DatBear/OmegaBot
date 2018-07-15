using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BattleNet.Connections;
using BattleNet.Logging;
using System.IO;
using BattleNet.Connections.Handlers;
using BattleNet.Strategies;

namespace BattleNet
{
    class GameThread
    {
        public GameData GameData { get; set; }

        private D2gsConnection _d2gsConnection;
        private D2gsHandler _d2gsHandler;

        private List<EventHandlerStrategy> _strategies = new List<EventHandlerStrategy>();

        public GameThread(D2gsConnection connection, D2gsHandler handler, UInt32 chickenLife, UInt32 potLife)
        {
            _d2gsConnection = connection;
            _d2gsHandler = handler;
            GameData = new GameData();
            GameData.ChickenLife = chickenLife;
            GameData.PotLife = potLife;
            _strategies.Add(new PartyStrategy(connection, GameData));
            _strategies.Add(new FollowStrategy(connection, GameData));
        }

        public static int Time()
        {
            return Environment.TickCount / 1000;
        }

        public AutoResetEvent _startRun;
        public void BotThread()
        {
            Logger.Write("Starting Bot thread");
            _startRun = new AutoResetEvent(false);
            _startRun.WaitOne();
            _strategies.ForEach(x => x.AddListeners(_d2gsHandler));
            while (true)
            {
                while (!GameData.ShouldLeaveGame) {

                    Thread.Sleep(100);
                }
                //Logger.Write("Signalled to start bot");
                //Int32 startTime = Time();
                //Logger.Write("Bot is in town.");
                //Thread.Sleep(3000);

                ////StashItems();

                //MoveToAct5();
                ///*
                //if (_gameData.WeaponSet != 0)
                //    WeaponSwap();
                //*/
                //DoPindle();

                //Logger.Write("Leaving the game.");
                //LeaveGame();
                //Thread.Sleep(500);

                //Int32 endTime = Time() - startTime;
                //Logger.Write("Run took {0} seconds.", endTime);
                //if (endTime < Settings.Instance.GameMinRuntime())
                //{
                //    Int32 waitTime = Settings.Instance.GameMinRuntime() - endTime;

                //    Logger.Write("waiting {0} seconds.", waitTime);
                //    Thread.Sleep(waitTime * 1000);
                //}
                _strategies.ForEach(x => x.RemoveListeners(_d2gsHandler));
                _startRun.WaitOne();
            }
        }

        public void SendPacket(byte command, params IEnumerable<byte>[] args)
        {
            _d2gsConnection.Write(_d2gsConnection.BuildPacket(command, args));
        }

        public void SendPacket(byte[] packet)
        {
            _d2gsConnection.Write(packet);
        }

        public void LeaveGame()
        {
            Logger.Write("Leaving the game.");
            _d2gsConnection.Write(_d2gsConnection.BuildPacket(0x69));

            Thread.Sleep(500);

            _d2gsConnection.Kill();
            
            //Status = ClientStatus.STATUS_NOT_IN_GAME;
        }

        public void MoveTo(UInt16 x, UInt16 y)
        {
            MoveTo(new Coordinate(x, y));
        }

        public void MoveTo(Coordinate target)
        {
            int time = Time();
            if (time - GameData.LastTeleport > 5)
            {
                SendPacket(Actions.Run(target));             
                GameData.LastTeleport = time;
                Thread.Sleep(120);
            }
            else
            {
                double distance = GameData.Me.Location.Distance(target);
                SendPacket(Actions.Run(target));
                Thread.Sleep((int)(distance * 80));
            }
            GameData.Me.Location = target;
        }

        public virtual void StashItems()
        {
            bool onCursor = false;
            List<Item> items;
            lock (GameData.Items)
            {
                items = new List<Item>(GameData.Items.Values);
            }
            foreach (Item i in items)
            {
                onCursor = false;

                if (i.action == (uint)Item.Action.to_cursor)
                    onCursor = true;
                else if (i.Container == Item.ContainerType.inventory)
                    onCursor = false;
                else
                    continue;

                if (i.type == "tbk" || i.type == "cm1" || i.type == "cm2")
                    continue;

                if (!GameData.Stash.FindFreeSpace(i, out var stashLocation))
                {
                    continue;
                }

                Logger.Write("Stashing item {0}, at {1}, {2}", i.name ?? i.type, stashLocation.X, stashLocation.Y);

                if (!onCursor)
                {
                    SendPacket(Actions.ItemToCursor(i.Id));
                    Thread.Sleep(500);
                }

                var stashPacket = Actions.ItemToContainer(i.Id, stashLocation, ContainerType.Stash);
                SendPacket(stashPacket);
                //SendPacket(0x18, BitConverter.GetBytes(i.Id), stashLocation.ToBytes(), new byte[] { 0x04, 0x00, 0x00, 0x00 });
                Thread.Sleep(400);
            }
        }

        public bool SwitchSkill(uint skill)
        {
            GameData.RightSkill = skill;
            SendPacket(Actions.SwitchSkill(skill));
            Thread.Sleep(100);
            return true;
        }

        public bool SwitchSkill(Skills.Type skill) {
            return SwitchSkill((uint) skill);
        }

        public bool GetAliveNpc(String name, double range, out NpcEntity output)
        {
            var n = (from npc in GameData.Npcs
                     where npc.Value.Name == name
                     && npc.Value.Life > 0
                     && (range == 0 || range > GameData.Me.Location.Distance(npc.Value.Location))
                     select npc).FirstOrDefault();
            if (n.Value == null)
            {
                output = default(NpcEntity);
                return false;
            }
            output = n.Value;
            return true;
        }

        public NpcEntity GetNpc(String name)
        {
            NpcEntity npc = GameData.Npcs.FirstOrDefault(n => n.Value.Name == name).Value;
            return npc;
        }

        public bool Attack(UInt32 id)
        {
            if (!_d2gsConnection.Socket.Connected)
                return false;
            GameData.CharacterSkillSetup = GameData.CharacterSkillSetupType.SORCERESS_LIGHTNING;
            switch (GameData.CharacterSkillSetup)
            {
                case GameData.CharacterSkillSetupType.SORCERESS_LIGHTNING:
                    if (GameData.RightSkill != (uint)Skills.Type.LIGHTNING)
                        SwitchSkill((uint)Skills.Type.LIGHTNING);
                    Thread.Sleep(300);
                    SendPacket(Actions.CastOnObject(id));
                    break;
                case GameData.CharacterSkillSetupType.SORCERESS_BLIZZARD:
                    if (GameData.RightSkill != (uint)Skills.Type.blizzard)
                        SwitchSkill((uint)Skills.Type.blizzard);
                    Thread.Sleep(300);
                    SendPacket(Actions.CastOnObject(id));
                    break;
                case GameData.CharacterSkillSetupType.SORCERESS_METEOR:
                    break;
                case GameData.CharacterSkillSetupType.SORCERESS_METEORB:
                    break;
            }
            return true;
        }

        public void MoveToAct5()
        {
            if (GameData.CurrentAct == Globals.ActType.ACT_I)
            {
                Logger.Write($"Moving to Act 5, {GameData.Me.Location} -> {GameData.RogueEncampmentWp.Location}," +
                             $"{GameData.Me.Location.Distance(GameData.RogueEncampmentWp.Location)}");
                MoveTo(GameData.RogueEncampmentWp.Location);
                byte[] temp = { 0x02, 0x00, 0x00, 0x00 };
                SendPacket(0x13, temp, BitConverter.GetBytes(GameData.RogueEncampmentWp.Id));
                Thread.Sleep(300);
                byte[] tempa = { 0x6D, 0x00, 0x00, 0x00 };
                SendPacket(0x49, BitConverter.GetBytes(GameData.RogueEncampmentWp.Id), tempa);
                Thread.Sleep(300);
                MoveTo(5105, 5050);
                MoveTo(5100, 5025);
                MoveTo(5096, 5018);
            }
        }

        public virtual void PickItems()
        {
            var pickingItems = (from i in GameData.Items
                                 where i.Value.Ground
                                 select i.Value);

            lock (GameData.Items)
            {
                foreach (var i in pickingItems)
                {
                    if (i.type != "mp5" && i.type != "hp5" && i.type != "gld" && i.type != "rvl" && i.quality > Item.QualityType.normal)
                    {
                        Logger.Write("{0}: {1}, {2}, Ethereal:{3}, {4}", i.name, i.type, i.quality, i.ethereal, i.sockets);
                    }
                }
                try
                {
                    foreach (var i in pickingItems)
                    {
                        if (!Pickit.PickitMap.ContainsKey(i.type))
                            continue;
                        if (GameData.Belt.Items.Count >= 16 && i.type == "rvl")
                            continue;
                        if (Pickit.PickitMap[i.type](i))
                        {
                            if (i.type != "gld" && i.type != "rvl")
                            {
                                Logger.Write("Picking up Item!");
                                Logger.Write("{0}: {1}, {2}, Ethereal:{3}, {4}", i.name, i.type, i.quality, i.ethereal, i.sockets);
                            }
                            SwitchSkill(0x36);
                            Thread.Sleep(200);
                            SendPacket(Actions.CastOnCoord((ushort)i.X, (ushort)i.Y));
                            Thread.Sleep(400);
                            byte[] tempa = { 0x04, 0x00, 0x00, 0x00 };
                            SendPacket(0x16, tempa, BitConverter.GetBytes((Int32)i.Id), GenericHandler.Nulls);
                            Thread.Sleep(500);
                            if (i.type != "rvl" && i.type != "gld")
                            {
                                using (StreamWriter sw = File.AppendText("log.txt"))
                                {
                                    sw.WriteLine("{6} [{5}] {0}: {1}, {2}, Ethereal:{3}, {4}", i.name, i.type, i.quality, i.ethereal, i.sockets == uint.MaxValue ? 0 : i.sockets, GameData.Me.Name, DateTime.Today.ToShortTimeString());
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Logger.Write("Failed to pickup items, something bad happened");
                }
            }
        }

        public void DoPindle()
        {
            UInt32 curLife = 0;

            MoveTo(5089, 5019);
            MoveTo(5090, 5030);
            MoveTo(5082, 5033);
            MoveTo(5074, 5033);

            if (!VisitMalah())
                return;

            MoveTo(5073, 5032);
            MoveTo(5073, 5044);
            MoveTo(5078, 5055);
            MoveTo(5081, 5065);
            MoveTo(5081, 5076);

            if (!ReviveMerc())
                return;

            MoveTo(5082, 5087);
            MoveTo(5085, 5098);
            MoveTo(5088, 5110);
            MoveTo(5093, 5121);
            MoveTo(5103, 5124);
            MoveTo(5111, 5121);

            EnterRedPortal();

            //Status = ClientStatus.STATUS_KILLING_PINDLESKIN;
            Logger.Write("Killing Pindleskin");

            //Precast();

            //teleport...
            //SwitchSkill((uint)Skills.Type.teleport);
            //Thread.Sleep(300);

            MoveTo(10064, 13268);
            Thread.Sleep(500);
            MoveTo(10061, 13260);
            Thread.Sleep(500);
            MoveTo(10058, 13236);
            Thread.Sleep(500);

            //SendPacket(Actions.CastOnCoord(10064, 13286));
            //Thread.Sleep(300);
            //SendPacket(Actions.CastOnCoord(10061, 13260));
            //Thread.Sleep(300);
            //SendPacket(Actions.CastOnCoord(10058, 13236));
            //Thread.Sleep(300);
           
            NpcEntity pindle = GetNpc("Pindleskin");
            if (pindle == default(NpcEntity))
            {
                Thread.Sleep(500);
                pindle = GetNpc("Pindleskin");
                if (pindle == default(NpcEntity))
                {
                    Logger.Write("Unable to find Pindleskin, probably got stuck.");
                    LeaveGame();
                    return;
                }
            }
            curLife = GameData.Npcs[pindle.Id].Life;
            if (GameData.Npcs[pindle.Id].IsLightning && GameData.CharacterSkillSetup == GameData.CharacterSkillSetupType.SORCERESS_LIGHTNING)
            {
                LeaveGame();
                return;
            }
            while (GameData.Npcs[pindle.Id].Life > 0 && _d2gsConnection.Socket.Connected)
            {
                if (!Attack(pindle.Id))
                {
                    LeaveGame();
                    return;
                }
                if (curLife > GameData.Npcs[pindle.Id].Life)
                {
                    curLife = GameData.Npcs[pindle.Id].Life;
                    //Console.WriteLine("{0}: [D2GS] Pindleskins Life: {1}", Account, curLife);
                }
            }
            Logger.Write("{0} is dead. Killing minions", pindle.Name);

            NpcEntity monster;
            while (GetAliveNpc("Defiled Warrior", 20, out monster) && _d2gsConnection.Socket.Connected)
            {
                curLife = GameData.Npcs[monster.Id].Life;
                Logger.Write("Killing Defiled Warrior");
                while (GameData.Npcs[monster.Id].Life > 0 && _d2gsConnection.Socket.Connected)
                {
                    if (!Attack(monster.Id))
                    {
                        LeaveGame();
                        return;
                    }
                    if (curLife > GameData.Npcs[monster.Id].Life)
                    {
                        curLife = GameData.Npcs[monster.Id].Life;
                        //Console.WriteLine("{0}: [D2GS] Defiled Warriors Life: {1}", Account, curLife);
                    }
                }
            }
            Logger.Write("Minions are dead, looting...");
            PickItems();

            //if (!TownPortal())
            //{
            //LeaveGame();
            //return;
            //}
            
        }
        public UInt32 GetSkillLevel(Skills.Type skill) {
            GameData.SkillLevels.TryGetValue(skill, out var skillLevel);
            GameData.ItemSkillLevels.TryGetValue(skill, out var itemSkillLevel);
            return skillLevel + itemSkillLevel;
        }

        public virtual bool UsePotion()
        {
            Item pot = (from n in GameData.Belt.Items
                        where n.type == "rvl"
                        select n).FirstOrDefault();

            if (pot == default(Item))
            {
                Logger.Write("No potions found in belt!");
                return false;
            }
            SendPacket(0x26, BitConverter.GetBytes(pot.Id), GenericHandler.Nulls, GenericHandler.Nulls);
            GameData.Belt.Items.Remove(pot);
            return true;
        }

        public bool VisitMalah()
        {
            NpcEntity malah = GetNpc("Malah");
            if (malah != null && malah != default(NpcEntity))
                TalkToTrader(malah.Id);
            else
            {
                LeaveGame();
                return false;
            }

            if (GetSkillLevel(Skills.Type.book_of_townportal) < 10)
            {
                Thread.Sleep(300);
                SendPacket(0x38, GenericHandler.One, BitConverter.GetBytes(malah.Id), GenericHandler.Nulls);
                Thread.Sleep(2000);
                Item n = (from item in GameData.Items
                          where item.Value.action == (uint)Item.Action.add_to_shop
                          && item.Value.type == "tsc"
                          select item).FirstOrDefault().Value;

                Logger.Write("Buying TPs");
                byte[] temp = { 0x02, 0x00, 0x00, 0x00 };
                for (int i = 0; i < 9; i++)
                {
                    SendPacket(0x32, BitConverter.GetBytes(malah.Id), BitConverter.GetBytes(n.Id), GenericHandler.Nulls, temp);
                    Thread.Sleep(200);
                }
                Thread.Sleep(500);
            }
            if (malah != null && malah != default(NpcEntity))
                SendPacket(0x30, GenericHandler.One, BitConverter.GetBytes(malah.Id));
            else
            {
                LeaveGame();
                return false;
            }

            Thread.Sleep(300);
            return true;
        }

        public bool TalkToTrader(UInt32 id)
        {
            GameData.TalkedToNpc = false;
            NpcEntity npc = GameData.Npcs[id];

            double distance = GameData.Me.Location.Distance(npc.Location);

            //if(debugging)
            Logger.Write("Attempting to talk to NPC");
            SendPacket(Actions.MakeEntityMove(id, GameData.Me.Location));
            
            int sleepStep = 200;
            for (int timeDifference = (int)distance * 120; timeDifference > 0; timeDifference -= sleepStep)
            {
                SendPacket(0x04, GenericHandler.One, BitConverter.GetBytes(id));
                Thread.Sleep(Math.Min(sleepStep, timeDifference));
            }

            SendPacket(0x13, GenericHandler.One, BitConverter.GetBytes(id));
            Thread.Sleep(200);
            SendPacket(0x2f, GenericHandler.One, BitConverter.GetBytes(id));

            int timeoutStep = 100;
            for (long npcTimeout = 4000; npcTimeout > 0 && !GameData.TalkedToNpc; npcTimeout -= timeoutStep)
                Thread.Sleep(timeoutStep);

            if (!GameData.TalkedToNpc)
            {
                Logger.Write("Failed to talk to NPC");
                return false;
            }
            return true;
        }

        public bool ReviveMerc()
        {
            if (!GameData.HasMerc && Settings.Instance.ResurrectMercenary())
            {
                Logger.Write("Reviving Merc");
                MoveTo(5082, 5080);
                MoveTo(5060, 5076);

                NpcEntity qual = GetNpc("Qual-Kehk");
                if (qual != null && qual != default(NpcEntity))
                    TalkToTrader(qual.Id);
                else
                {
                    LeaveGame();
                    return false;
                }
                byte[] three = { 0x03, 0x00, 0x00, 0x00 };
                SendPacket(0x38, three, BitConverter.GetBytes(qual.Id), GenericHandler.Nulls);
                Thread.Sleep(300);
                SendPacket(0x62, BitConverter.GetBytes(qual.Id));
                Thread.Sleep(300);
                SendPacket(0x38, three, BitConverter.GetBytes(qual.Id), GenericHandler.Nulls);
                Thread.Sleep(300);
                SendPacket(0x30, GenericHandler.One, BitConverter.GetBytes(qual.Id));
                Thread.Sleep(300);

                MoveTo(5060, 5076);
                MoveTo(5082, 5080);
                MoveTo(5081, 5076);
            }
            return true;
        }

        bool EnterRedPortal() {
            if (GameData.RedPortal == null) return false;
            Thread.Sleep(700);
            byte[] two = { 0x02, 0x00, 0x00, 0x00 };
            SendPacket(0x13, two, BitConverter.GetBytes(GameData.RedPortal.Id));
            Thread.Sleep(500);
            return true;
        }

    }
}
