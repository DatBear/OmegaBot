using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class Entity
    {
        protected Boolean _initialized;
        public Boolean Initialized { get { return _initialized; } set { _initialized = value; } }

        protected UInt32 _id;
        public UInt32 Id { get { return _id; } set { _id = value; } }

        protected Coordinate _location;
        public Coordinate Location { get { return _location; } set { _location = value; } }

        public Entity()
        {
            _initialized = false;
            _location = new Coordinate(0, 0);
        }

        public Entity(UInt32 id, UInt16 x, UInt16 y)
        {
            _initialized = true;
            _id = id;
            _location = new Coordinate(x, y);
        }
    
    }
}
