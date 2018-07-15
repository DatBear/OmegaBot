using BattleNet.Connections;
using BattleNet.Connections.Handlers;

namespace BattleNet.Strategies {
    public class AttackStrategy : EventHandlerStrategy {
        public AttackStrategy(D2gsConnection connection, GameData gameData) : base(connection, gameData) {
        }

        public override void AddListeners(D2gsHandler handler) {
            throw new System.NotImplementedException();
        }

        public override void RemoveListeners(D2gsHandler handler) {
            throw new System.NotImplementedException();
        }

        public override bool ShouldExecute(GameData gameData) {
            throw new System.NotImplementedException();
        }

        public override void Execute(GameData gameData) {
            throw new System.NotImplementedException();
        }
    }
}