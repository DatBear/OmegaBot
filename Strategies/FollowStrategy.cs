using BattleNet.Connections;
using BattleNet.Connections.Handlers;
using BattleNet.Logging;

namespace BattleNet.Strategies {
    public class FollowStrategy : EventHandlerStrategy {
        public FollowStrategy(D2gsConnection connection, GameData gameData) : base(connection, gameData) {
        }

        public override void AddListeners(D2gsHandler handler) {
            handler.UpdatePlayerPosition += OnUpdatePlayerPosition;
        }

        

        public override void RemoveListeners(D2gsHandler handler) {
            throw new System.NotImplementedException();
        }
        

        private void OnUpdatePlayerPosition(uint id, Coordinate coords, bool directoryknown) {
            var player = GameData.GetPlayer(id);
            if (player != null && player.Name == Settings.Instance.FollowPlayerName() && GameData.Me.Location.Distance(coords) > 5) {
                SendPacket(Actions.Run(coords));
            }

            Logger.Write($"Player: {id} moved to {coords}");
        }
    }
}