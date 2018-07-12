﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BattleNet
{
    class DataManager
    {
        private static DataManager s_instance;

        public static DataManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new DataManager();
                }
                return s_instance;
            }

        }
        public ItemDataType _itemData;
        public PlainTextDataType _experiences,
                            _magicalPrefixes,
                            _magicalSuffixes,
                            _rarePrefixes,
                            _rareSuffixes,
                            _uniqueItems,
                            _monsterNames,
                            _monsterFields,
                            _superUniques,
                            _itemProperties,
                            _skills;

        public DataManager(String dataDirectory = "data")
        {
            String[] fileNames =
            {
          		"experience.txt",
		        "magical_prefixes.txt",
		        "magical_suffixes.txt",
		        "rare_prefixes.txt",
		        "rare_suffixes.txt",
		        "unique_items.txt",
		        "monster_names.txt",
		        "monster_fields.txt",
		        "super_uniques.txt",
		        "ite_properties.txt",
		        "skills.txt"
            };

            String itemDataFile = Path.Combine(dataDirectory, "ite_data.txt");
            _itemData = new ItemDataType(itemDataFile);
            _experiences = new PlainTextDataType(Path.Combine(dataDirectory, fileNames[0]));
            _magicalPrefixes = new PlainTextDataType(Path.Combine(dataDirectory, fileNames[1]));
            _magicalSuffixes = new PlainTextDataType(Path.Combine(dataDirectory, fileNames[2]));
            _rarePrefixes = new PlainTextDataType(Path.Combine(dataDirectory, fileNames[3]));
            _rareSuffixes = new PlainTextDataType(Path.Combine(dataDirectory, fileNames[4]));
            _uniqueItems = new PlainTextDataType(Path.Combine(dataDirectory, fileNames[5]));
            _monsterNames = new PlainTextDataType(Path.Combine(dataDirectory, fileNames[6]));
            _monsterFields = new PlainTextDataType(Path.Combine(dataDirectory, fileNames[7]));
            _superUniques = new PlainTextDataType(Path.Combine(dataDirectory, fileNames[8]));
            _itemProperties = new PlainTextDataType(Path.Combine(dataDirectory, fileNames[9]));
            _skills = new PlainTextDataType(Path.Combine(dataDirectory, fileNames[10]));

            return;
        }

    }

    class ItemDataType
    {
        private List<ItemEntry> _items;
        public List<ItemEntry> Items { get { return _items; } }

        private ItemDataType()
        {
        }

        public ItemDataType(String file)
        {
            _items = new List<ItemEntry>();
            Dictionary<String, Item.ClassificationType> classificationMap = new Dictionary<string, Item.ClassificationType>();
            classificationMap["Amazon Bow"] = Item.ClassificationType.amazon_bow;
            classificationMap["Amazon Javelin"] = Item.ClassificationType.amazon_javelin;
            classificationMap["Amazon Spear"] = Item.ClassificationType.amazon_spear;
            classificationMap["Amulet"] = Item.ClassificationType.amulet;
            classificationMap["Antidote Potion"] = Item.ClassificationType.antidote_potion;
            classificationMap["Armor"] = Item.ClassificationType.armor;
            classificationMap["Arrows"] = Item.ClassificationType.arrows;
            classificationMap["Assassin Katar"] = Item.ClassificationType.assassin_katar;
            classificationMap["Axe"] = Item.ClassificationType.axe;
            classificationMap["Barbarian Helm"] = Item.ClassificationType.barbarian_helm;
            classificationMap["Belt"] = Item.ClassificationType.belt;
            classificationMap["Body Part"] = Item.ClassificationType.body_part;
            classificationMap["Bolts"] = Item.ClassificationType.bolts;
            classificationMap["Boots"] = Item.ClassificationType.boots;
            classificationMap["Bow"] = Item.ClassificationType.bow;
            classificationMap["Circlet"] = Item.ClassificationType.circlet;
            classificationMap["Club"] = Item.ClassificationType.club;
            classificationMap["Crossbow"] = Item.ClassificationType.crossbow;
            classificationMap["Dagger"] = Item.ClassificationType.dagger;
            classificationMap["Druid Pelt"] = Item.ClassificationType.druid_pelt;
            classificationMap["Ear"] = Item.ClassificationType.ear;
            classificationMap["Elixir"] = Item.ClassificationType.elixir;
            classificationMap["Gem"] = Item.ClassificationType.gem;
            classificationMap["Gloves"] = Item.ClassificationType.gloves;
            classificationMap["Gold"] = Item.ClassificationType.gold;
            classificationMap["Grand Charm"] = Item.ClassificationType.grand_charm;
            classificationMap["Hammer"] = Item.ClassificationType.hammer;
            classificationMap["Health Potion"] = Item.ClassificationType.health_potion;
            classificationMap["Helm"] = Item.ClassificationType.helm;
            classificationMap["Herb"] = Item.ClassificationType.herb;
            classificationMap["Javelin"] = Item.ClassificationType.javelin;
            classificationMap["Jewel"] = Item.ClassificationType.jewel;
            classificationMap["Key"] = Item.ClassificationType.key;
            classificationMap["Large Charm"] = Item.ClassificationType.large_charm;
            classificationMap["Mace"] = Item.ClassificationType.mace;
            classificationMap["Mana Potion"] = Item.ClassificationType.mana_potion;
            classificationMap["Necromancer Shrunken Head"] = Item.ClassificationType.necromancer_shrunken_head;
            classificationMap["Paladin Shield"] = Item.ClassificationType.paladin_shield;
            classificationMap["Polearm"] = Item.ClassificationType.polearm;
            classificationMap["Quest Item"] = Item.ClassificationType.quest_item;
            classificationMap["Rejuvenation Potion"] = Item.ClassificationType.rejuvenation_potion;
            classificationMap["Ring"] = Item.ClassificationType.ring;
            classificationMap["Rune"] = Item.ClassificationType.rune;
            classificationMap["Scepter"] = Item.ClassificationType.scepter;
            classificationMap["Scroll"] = Item.ClassificationType.scroll;
            classificationMap["Shield"] = Item.ClassificationType.shield;
            classificationMap["Small Charm"] = Item.ClassificationType.small_charm;
            classificationMap["Sorceress Orb"] = Item.ClassificationType.sorceress_orb;
            classificationMap["Spear"] = Item.ClassificationType.spear;
            classificationMap["Staff"] = Item.ClassificationType.staff;
            classificationMap["Stamina Potion"] = Item.ClassificationType.stamina_potion;
            classificationMap["Sword"] = Item.ClassificationType.sword;
            classificationMap["Thawing Potion"] = Item.ClassificationType.thawing_potion;
            classificationMap["Throwing Axe"] = Item.ClassificationType.throwing_axe;
            classificationMap["Throwing Knife"] = Item.ClassificationType.throwing_knife;
            classificationMap["Throwing Potion"] = Item.ClassificationType.throwing_potion;
            classificationMap["Tome"] = Item.ClassificationType.tome;
            classificationMap["Torch"] = Item.ClassificationType.torch;
            classificationMap["Wand"] = Item.ClassificationType.wand;

            List<string> lines = new List<string>();

            using (StreamReader r = new StreamReader(file))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            foreach (string line in lines)
            {
                try
                {
                    String[] tokens = line.Split('|');
                    if (tokens.Length == 0)
                        continue;
                    if (tokens.Length != 8)
                    {
                        Console.WriteLine("Invalid Token Count: {0}", tokens.Length);
                        throw new Exception("Unable to parse item data");
                    }
                    String name = tokens[0];
                    String code = tokens[1];
                    String classification_string = tokens[2];
                    UInt16 width = UInt16.Parse(tokens[3]);
                    UInt16 height = UInt16.Parse(tokens[4]);
                    bool stackable = UInt32.Parse(tokens[5]) != 0;
                    bool usable = UInt32.Parse(tokens[6]) != 0;
                    bool throwable = UInt32.Parse(tokens[7]) != 0;
                    Item.ClassificationType classification;
                    if (!classificationMap.TryGetValue(classification_string, out classification))
                        throw new Exception("Unable to parse item classification");
                    ItemEntry i = new ItemEntry(name, code, classification, width, height, stackable, usable, throwable);
                    _items.Add(i);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing ItemDataType: {0}", e.ToString());
                }
            }
        }

        public Boolean Get(String code, out ItemEntry output)
        {
            var items = from n in _items where n.Type == code select n;

            foreach (ItemEntry i in items)
            {
                output = i;
                return true;
            }
            output = null;
            return false;
        }

    }
    class PlainTextDataType
    {
        private List<String[]> _lines;

        public PlainTextDataType(String file)
        {
            _lines = new List<string[]>();
            List<string> lines = new List<string>();

            using (StreamReader r = new StreamReader(file))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            foreach (String line in lines)
            {
                String[] tokens = line.Split('|');
                _lines.Add(tokens);
            }
        }

        public Boolean Get(int offset, out String output)
        {
            if (offset < 0 || offset >= _lines.Count)
            {
                output = "";
                return false;
            }
            String[] line = _lines[offset];
            if (line.Length == 0)
                output = "";
            else
                output = line[0];
            return true;
        }

        public Boolean Get(int offset, out String[] output)
        {
            if (offset < 0 || offset >= _lines.Count)
            {
                output = null;
                return false;
            }
            output = _lines[offset];
            return true;
        }
    }

    class BinaryDataType
    {
        private List<byte> _data;
        public BinaryDataType(String file)
        {
            _data = new List<byte>(File.ReadAllBytes(file));
        }

        public Boolean Get(int offset, int length, out byte[] output)
        {
            if (offset < 0 || offset + length > _data.Count)
            {
                output = null;
                return false;
            }
            output = _data.GetRange(offset, length).ToArray();
            return true;
        }
    }

}
