using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class Globals
    {
        protected static String _binaryDirectory = "data";
        public static String BinaryDirectory { get { return _binaryDirectory; } set { _binaryDirectory = value; } }

        public static ushort GsPort { get { return (4000); } set { } }

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
