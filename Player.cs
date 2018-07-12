using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class Player : Entity
    {
        protected String _name;
        public String Name { get { return _name; } set { _name = value; } }

        protected Boolean _hasMercenary;
        public Boolean HasMecenary { get { return _hasMercenary; }  set { _hasMercenary = value; } }

        protected Boolean _directoryKnown;
        public Boolean DirectoryKnown { get { return _directoryKnown; } set { _directoryKnown = value; } }

        protected UInt32 _mercenaryId;
        public UInt32 MercenaryId { get { return _mercenaryId; } set  { _hasMercenary = true; _mercenaryId = value; } }

        protected Globals.CharacterClassType _class;
        public Globals.CharacterClassType Class { get { return _class; } set { _class = value; } }

        protected UInt32 _level;
        public UInt32 Level { get { return _level; } set { _level = value; } }

        protected UInt32 _portalId;
        public UInt32 PortalId { get { return _portalId; } set { _portalId = value; } }

        public Player()
        {
            _directoryKnown = false;
        }

        public Player(String name, UInt32 id, Globals.CharacterClassType class_, UInt32 level)
            : base(id, 0, 0)
        {
            _name = name;
            _class = class_;
            _level = level;
            _directoryKnown = false;
        }

        public Player(String name, UInt32 id, Globals.CharacterClassType class_, UInt32 level, UInt16 x, UInt16 y) 
            : base(id,x,y)
        {
            _name = name;
            _class = class_;
            _level = level;
            _directoryKnown = true;
        }


    }
}
