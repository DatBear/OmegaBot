using System;

namespace BattleNet {
    class Entity {
        public Boolean Initialized { get; set; }
        public UInt32 Id { get; set; }
        public Coordinate Location { get; set; }

        public Entity() {
            Initialized = false;
            Location = new Coordinate(0, 0);
        }

        public Entity(UInt32 id, UInt16 x, UInt16 y) {
            Initialized = true;
            Id = id;
            Location = new Coordinate(x, y);
        }

    }
}
