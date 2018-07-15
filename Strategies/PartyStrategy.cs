using System;
using BattleNet.Connections;
using BattleNet.Connections.Handlers;
using BattleNet.Enums;
using BattleNet.Logging;

namespace BattleNet.Strategies {
    public class PartyStrategy : EventHandlerStrategy {
        public PartyStrategy(D2gsConnection connection, GameData gameData) : base(connection, gameData) {
        }

        public override void AddListeners(D2gsHandler handler) {
            handler.PartyUpdateEvent += OnPartyUpdateEvent;
        }
        
        public override void RemoveListeners(D2gsHandler handler) {
            handler.PartyUpdateEvent -= OnPartyUpdateEvent;
        }

        public override bool ShouldExecute(GameData gameData) {
            throw new System.NotImplementedException();
        }

        public override void Execute(GameData gameData) {
            throw new System.NotImplementedException();
        }

        private void OnPartyUpdateEvent(int owner, PartyAction action) {
            //todo check options for auto accept?
            Logger.Write($"{nameof(PartyStrategy)}.{nameof(OnPartyUpdateEvent)}: {owner}, {action}");
            if (action == PartyAction.ServerInvite) {
                var packet = Actions.PartyAction(owner, PartyAction.ClientAcceptInvite);
                Logger.Write("Party Accept Packet? " + BitConverter.ToString(packet));
                SendPacket(packet);
            }
        }
    }
}