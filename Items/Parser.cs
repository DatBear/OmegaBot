using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet.Items
{
    class Parser
    {

        private delegate void ParseSection(BitReader reader, ref Item item);

        private static void GenericInfo(BitReader reader, ref Item item) // get basic info such as item
        {
            byte packet = (byte)reader.Read(8);
            item.action = (uint)reader.Read(8);
            item.Category = (uint)reader.Read(8);
            byte validSize = (byte)reader.Read(8);
            item.Id = (uint)reader.Read(32);
            if (packet == 0x9d)
            {
                reader.Read(40);
            }
        }

        private static void StatusInfo(BitReader reader, ref Item item) // get info for basic status info
        {
            item.Equipped = reader.ReadBit();
            reader.ReadBit();
            reader.ReadBit();
            item.InSocket = reader.ReadBit();
            item.Identified = reader.ReadBit();
            reader.ReadBit();
            item.SwitchedIn = reader.ReadBit();
            item.SwitchedOut = reader.ReadBit();
            item.Broken = reader.ReadBit();
            reader.ReadBit();
            item.Potion = reader.ReadBit();
            item.has_sockets = reader.ReadBit();
            reader.ReadBit();
            item.InStore = reader.ReadBit();
            item.NotInASocket = reader.ReadBit();
            reader.ReadBit();
            item.Ear = reader.ReadBit();
            item.StartItem = reader.ReadBit();
            reader.ReadBit();
            reader.ReadBit();
            reader.ReadBit();
            item.SimpleItem = reader.ReadBit();
            item.ethereal = reader.ReadBit();
            reader.ReadBit();
            item.Personalised = reader.ReadBit();
            item.Gambling = reader.ReadBit();
            item.RuneWord = reader.ReadBit();
            reader.Read(5);
            item.Version = (Item.VersionType)(reader.Read(8));
            reader.Read(2);
        }

        private static void GetLocation(BitReader reader, ref Item item)
        {
            byte destination = (byte)reader.Read(3);
            item.Ground = (destination == 0x03);

            if (item.Ground)
            {
                item.X = (UInt16)reader.Read(16);
                item.Y = (UInt16)reader.Read(16);
            }
            else
            {
                item.Directory = (byte)reader.Read(4);
                item.X = (byte)reader.Read(4);
                item.Y = (byte)reader.Read(3);
                item.Container = (Item.ContainerType)(reader.Read(4));
            }
            item.UnspecifiedDirectory = false;

            if (item.action == (uint)Item.Action.add_to_shop || item.action == (uint)Item.Action.remove_fro_shop)
            {
                long container = (long)(item.Container);
                container |= 0x80;
                if ((container & 1) != 0)
                {
                    container--; //remove first bit
                    item.Y += 8;
                }
                item.Container = (Item.ContainerType)container;
            }
            else if (item.Container == Item.ContainerType.unspecified)
            {
                if (item.Directory == (uint)Item.DirectoryType.not_applicable)
                {
                    if (item.InSocket)
                        //y is ignored for this container type, x tells you the index
                        item.Container = Item.ContainerType.item;
                    else if (item.action == (uint)Item.Action.put_in_belt || item.action == (uint)Item.Action.remove_fro_belt)
                    {
                        item.Container = Item.ContainerType.belt;
                        item.Y = item.X / 4;
                        item.X %= 4;
                    }
                }
                else
                    item.UnspecifiedDirectory = true;
            }
        }

        public static bool EarInfo(BitReader reader, ref Item item)
        {
            if (item.Ear)
            {
                reader.Read(3);
                item.EarLevel = (byte)reader.Read(7);
                //item.ear_name = "Fix Me"; //fix me later
                List<Byte> earName = new List<byte>();
                reader.Read(8);
                while (earName.Last() != 0x00)
                {
                    reader.Read(8); // 16 characters of 7 bits each for the name of the ear to process later
                }
                
                item.EarName = Convert.ToBase64String(earName.ToArray());
                return true;
            }
            else
                return false;
        }

        public static bool GetItemType(BitReader reader, ref Item item) // gets the 3 letter item code
        {
            byte[] codeBytes = new byte[4];
            for (int i = 0; i < codeBytes.Length; i++)
                codeBytes[i] = (byte)(reader.Read(8));
            codeBytes[3] = 0;

            item.type = Encoding.ASCII.GetString(codeBytes).Substring(0, 3);

            ItemEntry entry;
            if (!DataManager.Instance._itemData.Get(item.type, out entry))
            {
                Console.WriteLine("Failed to look up item in item data table");
                return true;
            }

            item.name = entry.Name;
            item.Width = entry.Width;
            item.Height = entry.Height;

            item.IsArmor = entry.IsArmor();
            item.IsWeapon = entry.IsWeapon();

            if (item.type == "gld")
            {
                item.IsGold = true;
                bool bigPile = reader.ReadBit();
                if (bigPile) item.Amount = (uint)reader.Read(32);
                else item.Amount = (uint)reader.Read(12);
                return true;
            }
            else return false;
        }

        public static void GetSocketInfo(BitReader reader, ref Item item)
        {
            item.UsedSockets = (byte)reader.Read(3);
        }

        public static bool GetLevelQuality(BitReader reader, ref Item item)
        {
            item.quality = Item.QualityType.normal;
            if (item.SimpleItem || item.Gambling)
                return false;
            item.Level = (byte)reader.Read(7);
            item.quality = (Item.QualityType)(reader.Read(4));
            return true;
        }

        public static void GetGraphicInfo(BitReader reader, ref Item item)
        {
            item.HasGraphic = reader.ReadBit();
            if (item.HasGraphic)
                item.Graphic = (byte)reader.Read(3);

            item.HasColour = reader.ReadBit();
            if (item.HasColour)
                item.Colour = (UInt16)reader.Read(11);
        }

        public static void GetIdentifiedInfo(BitReader reader, ref Item item)
        {
            if (item.Identified)
            {
                switch (item.quality)
                {
                    case Item.QualityType.inferior:
                        item.Prefix = (byte)reader.Read(3);
                        break;
                    case Item.QualityType.superior:
                        item.Superiority = (Item.SuperiorItemClassType)(reader.Read(3));
                        break;
                    case Item.QualityType.magical:
                        item.Prefix = (uint)reader.Read(11);
                        item.Suffix = (uint)reader.Read(11);
                        break;

                    case Item.QualityType.crafted:
                    case Item.QualityType.rare:
                        item.Prefix = (uint)reader.Read(8) - 156;
                        item.Suffix = (uint)reader.Read(8) - 1;
                        break;

                    case Item.QualityType.set:
                        item.SetCode = (uint)reader.Read(12);
                        break;
                    case Item.QualityType.unique:
                        if (item.type != "std") //standard of heroes exception?
                            item.UniqueCode = (uint)reader.Read(12);
                        break;
                }
            }

            if (item.quality == Item.QualityType.rare || item.quality == Item.QualityType.crafted)
            {
                for (ulong i = 0; i < 3; i++)
                {
                    if (reader.ReadBit())
                        item.Prefixes.Add((uint)reader.Read(11));
                    if (reader.ReadBit())
                        item.Suffixes.Add((uint)reader.Read(11));
                }
            }

            if (item.RuneWord)
            {
                item.RunewordId = (uint)reader.Read(12);
                item.RunewordParameter = (byte)reader.Read(4);
                //std::cout << "runeword_id: " << item.runeword_id << ", parameter: " << item.runeword_parameter << std::endl;
            }

            if (item.Personalised)
            {
                List<Byte> personalisedName = new List<byte>();
                reader.Read(8);
                while (personalisedName.Last() != 0x00)
                {
                    reader.Read(8); // 16 characters of 7 bits each for the name of the ear to process later
                }
                item.PersonalisedName = Convert.ToBase64String(personalisedName.ToArray()); //this is also a problem part i'm not sure about

            }

            if (item.IsArmor)
                item.Defense = (uint)reader.Read(11) - 10;

            if (item.type == "7cr")
                reader.Read(8);
            else if (item.IsArmor || item.IsWeapon)
            {
                item.MaximumDurability = (byte)reader.Read(8);
                item.Indestructible = (uint)((item.MaximumDurability == 0) ? 1 : 0);

                item.Durability = (byte)reader.Read(8);
                reader.ReadBit();
            }
            if (item.has_sockets)
                item.sockets = (byte)reader.Read(4);
        }
        public static Item Parse(List<byte> packet)
        {
            Item item = new Item();
            BitReader reader = new BitReader(packet.ToArray());
            try
            {
                GenericInfo(reader, ref item);
                StatusInfo(reader, ref item);
                GetLocation(reader, ref item);
                if (EarInfo(reader, ref item)) return item;
                if (GetItemType(reader, ref item)) return item;
                if (!GetLevelQuality(reader, ref item)) return item;
                GetGraphicInfo(reader, ref item);
                GetIdentifiedInfo(reader, ref item); // get nova to help with this
            }
            catch {
                // ignored
            }
            return item;
        }



    }
}