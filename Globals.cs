using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class Globals
    {
        public static String BinaryDirectory { get; set; } = "data";
        public static ushort GsPort => 4000;

        public enum ActType
        {
            ACT_I = 0,
            ACT_II = 1,
            ACT_III = 2,
            ACT_IV = 3,
            ACT_V = 4
        };

        public enum CharacterClassType
        {
            AMAZON,
            SORCERESS,
            NECROMANCER,
            PALADIN,
            BARBARIAN,
            DRUID,
            ASSASSIN
        };
    }
}
