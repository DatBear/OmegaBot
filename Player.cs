using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class Player : Entity
    {
        public String Name { get; set; }
        public Boolean HasMecenary { get; set; }
        public Boolean DirectoryKnown { get; set; }
        public UInt32 Level { get; set; }
        public UInt32 PortalId { get; set; }
        public Globals.CharacterClassType Class { get; set; }

        protected UInt32 _mercenaryId;
        public UInt32 MercenaryId { get { return _mercenaryId; } set  { HasMecenary = true; _mercenaryId = value; } }
        
        public Player()
        {
            DirectoryKnown = false;
        }

        public Player(String name, UInt32 id, Globals.CharacterClassType class_, UInt32 level)
            : base(id, 0, 0)
        {
            Name = name;
            Class = class_;
            Level = level;
            DirectoryKnown = false;
        }

        public Player(String name, UInt32 id, Globals.CharacterClassType class_, UInt32 level, UInt16 x, UInt16 y) 
            : base(id,x,y)
        {
            Name = name;
            Class = class_;
            Level = level;
            DirectoryKnown = true;
        }


    }
}
