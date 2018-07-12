using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleNet
{
    class NpcEntity : Entity
    {
        public string Name { get; set; }
        public uint Type { get; set; }
        public uint Life { get; set; }
        protected Coordinate TargetLocation;
        public bool Moving { get; set; }
        public bool Running { get; set; }
        public bool HasFlags { get; set; }
        public bool Champion { get; set; }
        public bool Unique { get; set; }
        public bool SuperUnique { get; set; }
        public bool IsMinion { get; set; }
        public bool Ghostly { get; set; }
        public bool IsLightning { get; set; }
        public int SuperUniqueId { get; set; }

        public NpcEntity()
        { }

        public NpcEntity(UInt32 id, UInt32 type, UInt32 life, UInt16 x, UInt16 y) :
            base(id,x,y)
        {
            Type = type;
            Life = life;
            HasFlags = false;
            Moving = false;
        }

    }
}
