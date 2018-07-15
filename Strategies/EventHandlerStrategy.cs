using BattleNet.Connections;
using BattleNet.Connections.Handlers;

namespace BattleNet.Strategies {
    public abstract class EventHandlerStrategy {
        protected D2gsConnection Connection { get; set; }
        protected GameData GameData { get; set; }

        public EventHandlerStrategy(D2gsConnection connection, GameData gameData) {
            Connection = connection;
            GameData = gameData;
        }

        public virtual void AddListeners(D2gsHandler handler) {
            
        }

        public virtual void RemoveListeners(D2gsHandler handler) {
            
        }

        public virtual bool ShouldExecute(GameData gameData) {
            return false;
        }

        public virtual void Execute(GameData gameData) {
            
        }

        protected void SendPacket(byte[] packet) {
            Connection.Write(packet);
        }
    }
}