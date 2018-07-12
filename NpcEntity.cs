using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class NpcEntity : Entity
    {
        protected String _name;
        public String Name { get { return _name; } set { _name = value; } } 

        protected UInt32 _type;
        public UInt32 Type { get { return _type; } set { _type = value; } }

        protected UInt32 _life;
        public UInt32 Life { get { return _life; } set { _life = value; } }

        protected Coordinate _targetLocation;
        public Coordinate TargetLocation { get { return _targetLocation; } set { _targetLocation = value; } }

        protected Boolean _moving;
        public Boolean Moving { get { return _moving; } set { _moving = value; } }

        protected Boolean _running;
        public Boolean Running { get { return _running; } set { _running = value; } }

        protected Boolean _hasFlags;
        public Boolean HasFlags { get { return _hasFlags; } set { _hasFlags = value; } }

        protected Boolean _flag1;
        public Boolean Champion { get { return _flag1; } set { _flag1 = value; } }

        protected Boolean _flag2;
        public Boolean Unique { get { return _flag2; } set { _flag2 = value; } }

        protected Boolean _superUnique;
        public Boolean SuperUnique { get { return _superUnique; } set { _superUnique = value; } }

        protected Boolean _isMinion;
        public Boolean IsMinion { get { return _isMinion; } set { _isMinion = value; } }

        protected Boolean _flag5;
        public Boolean Ghostly { get { return _flag5; } set { _flag5= value; } }

        protected Boolean _isLightning;
        public Boolean IsLightning { get { return _isLightning; } set { _isLightning = value; } }

        protected Int32 _superUniqueId;
        public Int32 SuperUniqueId { get { return _superUniqueId; }  set { _superUniqueId = value; } }

        public NpcEntity()
        { }

        public NpcEntity(UInt32 id, UInt32 type, UInt32 life, UInt16 x, UInt16 y) :
            base(id,x,y)
        {
            _type = type;
            _life = life;
            _hasFlags = false;
            _moving = false;
        }

    }
}
