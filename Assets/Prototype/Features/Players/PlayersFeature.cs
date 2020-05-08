using ME.ECS;

namespace Prototype.Features {
    
    using TState = PrototypeState;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class PlayersFeature : Feature<TState> {

        public Prototype.Features.Players.Data.FeaturePlayersData resourcesData { get; private set; }

        private RPCId createPlayerRpcId;
        private RPCId deletePlayerRpcId;
        public Photon.Realtime.Player activePlayer { get; private set; }
        
        protected override void OnConstruct(ref ConstructParameters parameters) {

            this.resourcesData = UnityEngine.Resources.Load<Prototype.Features.Players.Data.FeaturePlayersData>("FeaturePlayersData");

            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RegisterObject(this, 1);

            this.createPlayerRpcId = networkModule.RegisterRPC(new System.Action<int>(this.CreatePlayer_RPC).Method);
            this.deletePlayerRpcId = networkModule.RegisterRPC(new System.Action<int>(this.DeletePlayer_RPC).Method);

            this.AddSystem<Prototype.Features.Players.Systems.PlayerInitializationSystem>();
            this.AddSystem<Prototype.Features.Players.Systems.PlayerSelectionSystem>();

        }

        protected override void OnDeconstruct() {
            
            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.UnRegisterObject(this, 1);
            
        }

        /// <summary>
        /// Called on all clients, creating RPC call and send all clients info about new char
        /// </summary>
        /// <param name="player"></param>
        public void SetActivePlayer(Photon.Realtime.Player player) {

            this.activePlayer = player;

        }

        public void OnPlayerConnectedTimeSynced() {

            var player = this.activePlayer;
            UnityEngine.Debug.Log("OnPlayerConnected: " + player.ActorNumber);
            
            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.createPlayerRpcId, player.ActorNumber);
            if (PrototypeInitializer.playWithBot == true) networkModule.RPC(this, this.createPlayerRpcId, 2);

        }

        /// <summary>
        /// Called on host only, send all clients info about killed char
        /// </summary>
        public void OnPlayerDisconnected(Photon.Realtime.Player disconnectedPlayer) {

            UnityEngine.Debug.Log("OnPlayerDisconnected: " + disconnectedPlayer.ActorNumber);

            // Call rpc
            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.deletePlayerRpcId, disconnectedPlayer.ActorNumber);

        }

        public Entity GetPlayerEntityByActorId(int actorId) {

            var players = this.world.GetState().playerEntities;
            if (players.TryGetValue(actorId, out var ent) == true) {

                return ent;

            }
            
            return Entity.Empty;

        }

        private void DeletePlayer_RPC(int actorId) {
            
            UnityEngine.Debug.Log("DeletePlayer_RPC: " + actorId);

            // TODO: we need to remove all units
            var players = this.world.GetState().playerEntities;
            this.world.RemoveEntity<Player>(players[actorId]);
            players.Remove(actorId);

        }

        private void CreatePlayer_RPC(int actorId) {
            
            UnityEngine.Debug.Log("CreatePlayer_RPC: " + actorId);

            var player = this.world.AddEntity(new Player() { actorId = actorId });
            player.SetData(new Prototype.Features.Players.Components.InitializePlayer());

            var players = this.world.GetState().playerEntities;
            if (players.ContainsKey(actorId) == true) {
                
                players[actorId] = player;
                
            } else {

                players.Add(actorId, player);

            }

        }

    }

}